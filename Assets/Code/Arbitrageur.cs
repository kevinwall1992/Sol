using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Arbitrageur : User.Script
{
    public IEnumerable<Craft> TransportCrafts
    { get { return User.Crafts.Where(craft => craft.HasNavigation); } }


    private void Update()
    {
        if (TransportCrafts.Count() == 0)
            return;

        foreach (Craft craft in TransportCrafts)
        {
            if (craft.Navigation.UpcomingTransfers.Count() == 0)
            {
                if (craft.Station == null)
                    continue;

                Market market = craft.Station.OfficialMarket;

                Plan plan = GetPlan(craft, market);
                if (plan == null)
                    continue;

                List<System.Action> sale_actions = new List<System.Action>();
                List<System.Action> purchase_actions = new List<System.Action>();

                foreach (string item_name in plan.Requisition.Keys)
                {
                    float target_quantity = plan.Requisition[item_name];
                    float existing_quantity = craft.Cargo.GetQuantity(item_name);

                    if (target_quantity > existing_quantity)
                        purchase_actions.Add(() => 
                            market.Purchase(User,
                                            craft.Cargo,
                                            item_name,
                                            target_quantity - existing_quantity));

                    else if (target_quantity < existing_quantity)
                    {
                        Item item = craft.Cargo.Retrieve(
                            item_name, 
                            existing_quantity - target_quantity);

                        sale_actions.Add(() => market.Sell(
                            User, 
                            craft.Cargo, 
                            item_name,
                            existing_quantity - target_quantity));
                    }
                }

                foreach (System.Action sale_action in sale_actions)
                    sale_action();
                foreach (System.Action purchase_action in purchase_actions)
                    purchase_action();

                craft.Navigation.AddTransfer(GetTransfer(craft, plan.SaleMarket));
            }
        }
    }

    Plan GetPlan(Craft craft, Market here)
    {
        IEnumerable<Market> markets =
            Scene.The.Stations.Select(station => station.OfficialMarket)
            .Where(market => market != here);

        Dictionary<string, float> owned_quantities = craft.Cargo.Items
            .Select(item => item.Name)
            .RemoveDuplicates()
            .ToDictionary(item_name => item_name, 
                          item_name => craft.Cargo.GetQuantity(item_name));

        Dictionary<string, float> available_quantities =
            new Dictionary<string, float>(owned_quantities);

        Dictionary<string, Item> samples =
            owned_quantities.Keys.ToDictionary(
                item_name => item_name,
                item_name => craft.Cargo.GetSampleItem(item_name));

        foreach (Item sample in here.Wares)
        {
            if (!owned_quantities.ContainsKey(sample.Name))
                owned_quantities[sample.Name] = 0;

            if (!available_quantities.ContainsKey(sample.Name))
                available_quantities[sample.Name] = 0;
            available_quantities[sample.Name] += here.GetTotalSupply(sample.Name);

            samples[sample.Name] = sample;
        }

        string propellent_name = craft.Engine.Propellent.Name;

        List<float> target_cargo_masses = new List<float>();
        for (int i = 0; i < 20; i++)
            target_cargo_masses.Add(0.1f * craft.CurbMass * Mathf.Pow(1.6f, i));


        Plan best_plan = null;
        float best_profit = 0;

        //Find best Plan for each payload size
        foreach (float target_cargo_mass in target_cargo_masses)
        {
            //Find best Plan for each market
            foreach(Market there in markets)
            {
                Plan plan = new Plan(there, samples.Keys);

                float cargo_mass = 0;


                //Compute propellent requirements to reach target market
                float propellent_mass_required = 
                    craft.Engine.GetPropellentMassRequired(
                        GetTransfer(craft, there),
                        craft.CurbMass + target_cargo_mass);
                float propellent_quantity_required =
                    craft.Engine.PropellentMassToUnits(propellent_mass_required);
                plan.Requisition[propellent_name] += propellent_quantity_required;

                System.Func<Item, float> GetMarginalQuantity = delegate (Item item)
                {
                    float mass_fraction = 
                        Mathf.Min(target_cargo_mass / 10, target_cargo_mass - cargo_mass) /
                        item.Physical().MassPerUnit;

                    float supply_fraction = available_quantities[item.Name] / 10;

                    return Mathf.Min(mass_fraction, supply_fraction);
                };

                System.Func<string, float, float> GetProfit = 
                delegate (string item_name, float quantity)
                {
                    float owned_quantity = owned_quantities[item_name];
                    float sell_here_quantity = 0;
                    float sell_there_quantity = quantity;

                    if (item_name == propellent_name)
                        sell_there_quantity -= propellent_quantity_required;

                    if (owned_quantity > quantity)
                        sell_here_quantity = owned_quantity - quantity;


                    float sale_value =
                        here.GetSaleValue(item_name, sell_here_quantity) +
                        there.GetSaleValue(item_name, sell_there_quantity);

                    float purchase_cost = 0;
                    if (quantity > owned_quantity)
                        purchase_cost = 
                            here.GetPurchaseCost(item_name, quantity - owned_quantity);

                    return sale_value - purchase_cost;
                };

                System.Func<string, float, float> GetTransactionBalance =
                delegate (string item_name, float quantity)
                {
                    float owned_quantity = owned_quantities[item_name];

                    if (quantity > owned_quantity)
                        return -here
                            .GetPurchaseCost(item_name, quantity - owned_quantity);

                    else if (quantity < owned_quantity)
                        return here
                            .GetSaleValue(item_name, owned_quantity - quantity);

                    return 0;
                };

                System.Func<Item, float> GetMarginalProfit = delegate (Item item)
                {
                    float planned_quantity = plan.Requisition[item.Name];
                    float marginal_quantity = GetMarginalQuantity(item);

                    float planned_profit = GetProfit(item.Name, planned_quantity);
                    float prospective_profit = GetProfit(item.Name, planned_quantity + 
                                                         marginal_quantity);

                    return prospective_profit - planned_profit;
                };


                //Purchase small quantity of most profitable item until full
                while (cargo_mass + float.Epsilon < target_cargo_mass)
                {
                    Item best_item = samples.Values
                        .Where(item => available_quantities[item.Name] > float.Epsilon)
                        .Sorted(item => GetMarginalProfit(item))
                        .Reversed()
                        .FirstOrDefault();

                    if (best_item == null)
                        break;

                    float marginal_quantity = GetMarginalQuantity(best_item);

                    plan.Requisition[best_item.Name] += marginal_quantity;
                    cargo_mass += marginal_quantity * best_item.Physical().MassPerUnit;
                }
                if (cargo_mass + float.Epsilon < target_cargo_mass)
                    continue;

                float transaction_balance = plan.Requisition
                    .Sum(pair => GetTransactionBalance(pair.Key, pair.Value));

                if (-transaction_balance > User.PrimaryBankAccount.Balance)
                    continue;

                float profit = plan.Requisition
                    .Sum(pair => GetProfit(pair.Key, pair.Value));

                if (profit > best_profit)
                {
                    best_plan = plan;
                    best_profit = profit;
                }
            }
        }

        return best_plan;
    }

    Navigation.Transfer GetTransfer(Craft craft, Market market)
    {
        return new InterplanetaryTransfer(craft.Motion,
                                          market.Station.GetVisitingMotion(craft),
                                          Scene.The.Clock.Now);
    }


    class Plan
    {
        public Market SaleMarket;

        public Dictionary<string, float> Requisition = 
            new Dictionary<string, float>();

        public Plan(Market target_market, IEnumerable<string> available_items)
        {
            SaleMarket = target_market;

            Requisition = available_items.ToDictionary(
                item_name => item_name, 
                item_name => 0.0f);
        }
    }
}
