﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Manufacturer : User.Script
{
    System.DateTime last_purchase_date = System.DateTime.MinValue;

    public float MeetingsPerYear;
    public float DaysBetweenMeetings { get { return 365 / MeetingsPerYear; } }

    public IEnumerable<Machine> Machines
    {
        get
        {
            return Scene.The.Stations.SelectMany(station => station.GetRooms(User))
                .SelectMany(room => room.Container.Items.Values
                    .SelectComponents<Item, Machine>());
        }
    }

    private void Update()
    {
        float days_since_last_meeting = 
            (float)(Scene.The.Clock.Now - last_purchase_date).TotalDays;
        if (days_since_last_meeting < DaysBetweenMeetings)
            return;
        last_purchase_date = Scene.The.Clock.Now;

        SellGoods();
        SellMachines();

        BuyMaterials();
        BuyMachines();
        BuyMaterials();
    }

    void SellGoods()
    {
        foreach (Station station in Scene.The.Stations)
        {
            Storage room_storage = new Storage(
                station.GetRooms(User).Select(room => room.Container));

            IEnumerable<Item> goods = station.GetStorage(User).Items.Where(
                item => !item.HasComponent<Machine>());

            foreach (Item item in goods)
                station.OfficialMarket.Sell(User, item);
        }
    }

    void SellMachines()
    {
        foreach (Machine machine in Machines)
        {
            Station station = machine.Item.Station();

            if (GetProfitPerMachinePerDay(machine, machine.Item.Station()) < 0)
            {
                Storage storage = station.GetStorage(User);
                Item machines_to_sell = 
                    storage.Retrieve(machine.name, 
                                     0.2f * storage.GetQuantity(machine.Item.name));

                storage.Store(station.OfficialMarket.Sell(User, machines_to_sell));
            }
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
            Storage storage = station.GetStorage(User);

            foreach (string input_name in machine.Recipe.Inputs.Keys)
            {
                float quantity = machine.Storage.GetQuantity(input_name);

                float target_quantity = machine.Recipe.Inputs[input_name] *
                                    machine.CyclesPerDay *
                                    storage.GetQuantity(machine.Item.Name) * 
                                    DaysBetweenMeetings;

                float purchase_quantity = target_quantity - quantity;

                if (purchase_quantity <= 0)
                    continue;

                float maximum_purchase_quantity = station.OfficialMarket
                    .GetPurchaseQuantity(input_name, User.PrimaryBankAccount.Balance);
                purchase_quantity =
                    Mathf.Min(purchase_quantity,
                                maximum_purchase_quantity);

                station.OfficialMarket.Purchase(
                    User,
                    input_name,
                    purchase_quantity,
                    machine.Storage);
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

            foreach (Station station in Scene.The.Stations)
            {
                Storage storage = station.GetStorage(User);
                if (station.GetRooms(User).Count() == 0)
                    continue;

                IEnumerable<Machine> machines_for_sale =
                    station.OfficialMarket.GetSampleItems<Machine>();

                foreach (Machine machine in machines_for_sale)
                {
                    float profit_per_day = GetProfitPerMachinePerDay(machine, station);

                    float purchase_quantity_ = station.OfficialMarket
                        .GetPurchaseQuantity(machine.Item.Name, target_purchase_cost);
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
                .GetPurchaseQuantity(best_machine.Item.Name, target_purchase_cost);

            Storage room_storage = new Storage(
                    best_station.GetRooms(User).Select(room => room.Container));

            best_station.OfficialMarket
                .Purchase(User,
                          best_machine.Item.Name,
                          purchase_quantity,
                          room_storage);
        }

        new Storage(Scene.The.Stations
            .SelectMany(station => station.GetRooms(User)).Select(room => room.Container))
            .TouchItems(item => item.GetComponent<Machine>().IsOn = true,
                        item => item.HasComponent<Machine>());
    }

    float GetProfitPerMachinePerDay(Machine machine, Station station)
    {
        float input_costs_per_day = 0;
        foreach (string input_name in machine.Recipe.Inputs.Keys)
        {
            float quantity_per_day = machine.Recipe.Inputs[input_name] *
                                     machine.CyclesPerDay;

            input_costs_per_day += station.OfficialMarket
                .GetPurchaseCost(input_name, quantity_per_day * DaysBetweenMeetings) / 
                DaysBetweenMeetings;
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
            .GetSaleValue(machine.Recipe.Output.Name,
                          machine.Recipe.OutputQuantity * machine.CyclesPerDay);

        return sale_value_per_day - total_costs_per_day;
    }
}