using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;


public class Arbitrageur : Division
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

                Market here = craft.Station.OfficialMarket;

                (Manifest shopping_list, Market there) = 
                    MakeShoppingList(craft, here);
                if (shopping_list == null)
                    continue;

                List<Action> sale_actions = new List<Action>();
                List<Action> purchase_actions = new List<Action>();

                foreach (Item item in shopping_list.Items)
                {
                    float target_quantity = shopping_list.GetQuantity(item);
                    float existing_quantity = craft.Cargo.GetQuantity(item.Name);

                    if (target_quantity > existing_quantity)
                        purchase_actions.Add(() => 
                            here.Purchase(User,
                                            craft.Cargo,
                                            item.Name,
                                            target_quantity - existing_quantity));

                    else if (target_quantity < existing_quantity)
                        sale_actions.Add(() => here.Sell(
                            User, 
                            craft.Cargo,
                            item.Name,
                            existing_quantity - target_quantity));
                }

                foreach (Action sale_action in sale_actions)
                    sale_action();
                foreach (Action purchase_action in purchase_actions)
                    purchase_action();

                craft.Navigation.AddTransfer(GetTransfer(craft, there));
            }
        }
    }

    (Manifest, Market) MakeShoppingList(Craft craft, Market here)
    {
        IEnumerable<Market> markets =
            The.Stations.Select(station => station.OfficialMarket)
            .Where(market => market != here);

        IEnumerable<Item> products = craft.Cargo.GetSampleItems()
            .Union(here.Wares, item => item.Name);

        Dictionary<string, float> owned_quantities = products
            .ToDictionary(item => item.Name,
                          item => craft.Cargo.GetQuantity(item.Name));

        float propellent_available =
            here.GetTotalSupply(craft.Engine.Propellent.Name);

        float maximum_cost =
            0.9f *
            User.PrimaryBankAccount.Balance /
            User.Crafts.Count();


        var best_shopping_lists =
            The.Stations.Where(station => station.OfficialMarket != here)
            .Select(station =>
        {
            Market there = station.OfficialMarket;

            Navigation.Transfer transfer = GetTransfer(craft, there);

            List<float> cargo_masses = new List<float>();
            for (int i = 0; i < 20; i++)
                cargo_masses.Add(0.1f * craft.CurbMass * Mathf.Pow(1.6f, i));


            //For each destination Market, create an Arbitrage describing the cost
            //to purchase goods here and the value of selling goods there.

            Arbitrage arbitrage = new Arbitrage(
                (item, quantity) =>
                {
                    float opportunity_cost = 0;
                    if (owned_quantities[item.Name] > 0)
                    {
                        float withheld_quantity =
                            Mathf.Min(owned_quantities[item.Name], quantity);

                        opportunity_cost +=
                            here.GetSaleValue(item.Name, withheld_quantity);

                        quantity -= withheld_quantity;
                    }

                    float purchase_cost =
                        here.GetPurchaseCost(item.Name, quantity);

                    return opportunity_cost + purchase_cost;
                },
                (item, quantity) => there.GetSaleValue(item.Name, quantity));


            //For each arbitrage, create shopping lists for each combination of 
            //journey hyperparameters

            IEnumerable<(Manifest ShoppingList,
                         Market There,
                         float ROI)> shopping_lists =
            cargo_masses.Select(cargo_mass =>
            {
                float propellent_mass_required =
                    craft.Engine.GetPropellentMassRequired(
                        transfer,
                        craft.CurbMass + cargo_mass);
                float propellent_required =
                    craft.Engine.PropellentMassToUnits(propellent_mass_required);
                if (propellent_required > propellent_available)
                    return (null, null, float.NegativeInfinity);

                //Equipment are items needed on board that are _not_ consumed
                Manifest equipment = new Manifest();

                //Provisions are items needed on board that _are_ consumed
                Manifest provisions = new Manifest();
                provisions.Add(craft.Engine.Propellent, 
                                       propellent_required);
                float fixed_costs = provisions.Items
                    .Sum(item => arbitrage.GetPurchaseCost(item, provisions[item]));

                Func<Item, float, float> GetTransportCosts = 
                    arbitrage.CreateTransportCostFunction(provisions);

                Manifest shopping_list = arbitrage.MakeShoppingList(
                    products,
                    GetTransportCosts,
                    craft.Cargo, equipment.Collated(provisions),
                    here, maximum_cost - fixed_costs,
                    new Arbitrage.LinearSpace(
                        cargo_mass, product => product.GetMassPerUnit()),
                    fixed_costs);

                float roi = arbitrage.GetROIPerYear(
                    shopping_list, 
                    fixed_costs, 
                    (float)(transfer.ArrivalDate - The.Clock.Now).TotalDays / 365,
                    GetTransportCosts);
                if(roi < 0)
                    return (null, null, float.NegativeInfinity);

                shopping_list.Add(provisions);

                return (shopping_list, there, roi);
            });


            //Select best shopping list for each arbitrage by computing its ROI
            //per year. 

            return shopping_lists.MaxElement(t => t.ROI);
        });


        //Finally, select best overall shopping list and destination market. 

        return best_shopping_lists
            .MaxElement(t => t.ROI)
            .Select((shopping_list, market, roi) => 
                    (shopping_list, market));
    }

    Navigation.Transfer GetTransfer(Craft craft, Market market)
    {
        return new InterplanetaryTransfer(craft.Motion,
                                          market.Station.GetVisitingMotion(craft),
                                          The.Clock.Now);
    }
}
