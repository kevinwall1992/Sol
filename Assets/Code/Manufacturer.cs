using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Manufacturer : Division
{
    public IEnumerable<Machine> Machines
    {
        get
        {
            return The.Stations
                .Select(station => User.GetInventory(station))
                .SelectMany(inventory => inventory.Items)
                .SelectComponents<Item, Machine>();
        }
    }

    protected override void ConductBusiness()
    {
        SellGoods();
        SellMachines();

        BuyMaterials();
        BuyMachines();
        BuyMaterials();
    }

    void SellGoods()
    {
        foreach (Station station in The.Stations)
        {
            Inventory inventory = User.GetInventory(station);

            IEnumerable<Item> goods = inventory.Items.Where(
                item => !item.HasComponent<Machine>());

            foreach (Item item in goods)
            {
                float quantity = Mathf.Min(
                    station.OfficialMarket.GetTotalDemand(item),
                    item.Quantity);

                station.OfficialMarket.Sell(User, inventory, item, quantity);
            }
        }
    }

    void SellMachines()
    {
        foreach (Machine machine in Machines)
        {
            Station station = machine.Item.Station();
            Inventory inventory = User.GetInventory(station);
            float quantity = inventory.GetQuantity(machine.Item);

            if (GetProfitPerMachinePerDay(machine, machine.Item.Station()) < 0)
                station.OfficialMarket.Sell(
                    User, inventory, 
                    machine.Item, 0.2f * quantity);
        }
    }

    void BuyMaterials()
    {
        List<Machine> sorted_machines = Machines
            .Sorted(machine => GetProfitPerMachinePerDay(machine, machine.Item.Station()))
            .Reversed();

        foreach (Machine machine in sorted_machines)
        {
            Station station = machine.Item.Station();
            Inventory inventory = User.GetInventory(station);

            foreach (Item input in machine.Recipe.Inputs.Samples)
            {
                float quantity = inventory.GetQuantity(input);

                float target_quantity = machine.Recipe.Inputs[input] *
                                    machine.CyclesPerDay *
                                    inventory.GetQuantity(machine.Item) * 
                                    Meeting.DaysBetweenSessions;

                float purchase_quantity = target_quantity - quantity;

                if (purchase_quantity <= 0)
                    continue;

                float maximum_purchase_quantity = station.OfficialMarket
                    .GetPurchasableQuantity(input, User.PrimaryBankAccount.Balance);
                purchase_quantity =
                    Mathf.Min(purchase_quantity,
                                maximum_purchase_quantity);

                station.OfficialMarket.Purchase(
                    User,
                    inventory,
                    input,
                    purchase_quantity);
            }
        }
    }

    void BuyMachines()
    {
        int granularity = 10;
        float target_purchase_cost = 0.3f * User.PrimaryBankAccount.Balance / granularity;

        for (int i = 0; i < granularity; i++)
        {
            Station best_station = null;
            Machine best_machine = null;
            float best_roi_per_day = 0;

            foreach (Station station in The.Stations)
            {
                if (station.GetRooms(User).Count() == 0)
                    continue;

                IEnumerable<Machine> machines_for_sale = station.OfficialMarket.Wares
                    .SelectComponents<Item, Machine>();

                foreach (Machine machine in machines_for_sale)
                {
                    float profit_per_day = GetProfitPerMachinePerDay(machine, station);

                    float purchase_quantity_ = station.OfficialMarket
                        .GetPurchasableQuantity(machine.Item, target_purchase_cost);
                    float quantity_per_credit = purchase_quantity_ /
                                                target_purchase_cost;

                    float roi_per_day = profit_per_day *
                                        quantity_per_credit;

                    if (roi_per_day > best_roi_per_day)
                    {
                        best_station = station;
                        best_machine = machine;
                    }
                }
            }


            if (best_machine == null)
                break;

            float purchase_quantity = best_station.OfficialMarket
                .GetPurchasableQuantity(best_machine.Item, target_purchase_cost);

            best_station.OfficialMarket
                .Purchase(User,
                          User.GetInventory(best_station),
                          best_machine.Item,
                          purchase_quantity);
        }

        foreach(Station station in The.Stations)
            User.GetInventory(station)
                .Touch(item => item.GetComponent<Machine>().IsOn = true,
                       item => item.HasComponent<Machine>());
    }

    float GetProfitPerMachinePerDay(Machine machine, Station station)
    {
        float input_costs_per_day = 0;

        foreach (Item input in machine.Recipe.Inputs.Samples)
        {
            float quantity_per_day = machine.Recipe.Inputs[input] *
                                     machine.CyclesPerDay;

            input_costs_per_day += station.OfficialMarket
                .GetPurchaseCost(input, quantity_per_day * 
                                        Meeting.DaysBetweenSessions) / 
                Meeting.DaysBetweenSessions;
        }

        float electricity_costs_per_day = 0;
        float maintenance_costs_per_day = 0;
        float insurance_costs_per_day = 0;
        float replacement_costs_per_day = 0;

        float total_costs_per_day = input_costs_per_day +
                                    electricity_costs_per_day +
                                    maintenance_costs_per_day +
                                    insurance_costs_per_day +
                                    replacement_costs_per_day;

        float sale_value_per_day = station.OfficialMarket
            .GetSaleValue(machine.Recipe.Output,
                          machine.Recipe.OutputQuantity * machine.CyclesPerDay);

        return sale_value_per_day - total_costs_per_day;
    }
}
