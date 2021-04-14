using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

//An "Arbitrage" represents the price differential of products between a 
//source "market" and a destination "market".

//Upon this foundation, an optimal list of goods to transport between these 
//markets can be created, provided the fixed and variable costs, as well as the
//real and abstract spatial constraints on the journey. (f.e. volume of hold, 
//budget, etc.). 
//Finally, the yearly roi of a journey can be calculated from a list of goods
//and the fixed and variable costs. 

//Note that an Arbitrage can be considered abstractly; one could think 
//of "purchasing" as the variable costs associated with manufacturing a
//product. In that sense, the source "market" may be considered to be your
//factory. Further, if you can put a dollar value on sinking your products
//anywhere, whether it be an actual destination Market, or if you are planning
//on consuming the product yourself, then this class may still be a good fit. 

//The financial terms used throughout this class refer to the journey, not the 
// business as a whole, therefore,
//"fixed costs" refer to fixed costs of the journey, and
//"variable costs" refer to costs that scale with items transported.
//"transport costs" are defined as [variable costs - purchase cost]


public partial class Arbitrage
{
    public Func<Item, float, float> GetPurchaseCost;
    public Func<Item, float, float> GetSaleValue;

    public Arbitrage(Func<Item, float, float> GetPurchaseCost_,
                     Func<Item, float, float> GetSaleValue_)
    {
        GetPurchaseCost = GetPurchaseCost_;
        GetSaleValue = GetSaleValue_;
    }

    public float GetROIPerYear(Manifest shopping_list, 
                               float fixed_costs, 
                               float transit_time_in_years,
                               Func<Item, float, float> GetTransportCosts = null)
    {
        if (GetTransportCosts == null)
            GetTransportCosts = (item, quantity) => 0;

        float total_costs = fixed_costs;
        float sale_value = 0;

        foreach (Item item in shopping_list.Items)
        {
            float quantity = shopping_list.GetQuantity(item);

            total_costs += GetPurchaseCost(item, quantity) + 
                           GetTransportCosts(item, quantity);

            sale_value += GetSaleValue(item, quantity);
        }

        float roi = (sale_value - total_costs) / total_costs;

        return Mathf.Pow(1 + roi, 1 / transit_time_in_years) - 1;
    }
}
