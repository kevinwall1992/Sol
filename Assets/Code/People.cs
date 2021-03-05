using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;


[RequireComponent(typeof(PhysicalItem))]
[RequireComponent(typeof(PerishableItem))]

public class People : User.Script
{
    public float AverageIncomePerYear;

    public List<ItemNeed> ItemNeeds = new List<ItemNeed>();

    public Meeting ShoppingTrip;

    public float Population
    {
        get { return Item.Quantity; }
        set { Item.Quantity = value; }
    }

    public Storage Housing
    {
        get
        {
            return new Storage(Item.Station().GetRooms(User)
                .Where(room => room.HasComponent<Housing>())
                .Select(room => room.Container));
        }
    }

    public Market Market { get { return Item.Station().OfficialMarket; } }

    public Storage Storage
    { get { return Item.Station().GetStorage(User); } }

    public Item Item { get { return GetComponent<Item>(); } }

    private void Start()
    {
        ShoppingTrip.AddTask(Live);
    }

    private void Update()
    {
        
    }

    void Live()
    {
        //Get paid

        User.PrimaryBankAccount.Deposit((ShoppingTrip.DaysBetweenSessions / 365) * 
                                        AverageIncomePerYear * 
                                        Population);


        //Buy groceries

        int granularity = 10;
        float credits = User.PrimaryBankAccount.Balance / 10;

        for (int i = 0; i < granularity; i++)
        {
            ItemNeed most_important_need = ItemNeeds
                .Sorted(need => need.GetMarginalUtilityOfPurchase(this, credits))
                .Last();

            if (most_important_need.GetMarginalUtilityOfPurchase(this, credits) <= 0)
                break;

            float quantity = 
                Market.GetPurchaseQuantity(most_important_need.ItemName, credits);

            Market.Purchase(User,
                            Storage,
                            most_important_need.ItemName, 
                            quantity);
        }

        float population_change = GetPopulationChange();


        //Nom nom

        foreach (ItemNeed need in ItemNeeds)
            Storage.TouchItems(item => item.Quantity = 0, 
                               item => item.Name == need.ItemName && 
                                       !item.HasComponent<Housing>());


        //Get laid (/ to rest)

        Population += population_change;
    }

    public float GetPopulationChange()
    {
        return GetPopulationChange(null, 0);
    }

    public float GetPopulationChange(PurchasableNeed marginal_need, float credits)
    {
        float growth_rate = 0;
        float growth_rate_modifier = 1;
        float death_rate = 0;

        foreach (Need need in ItemNeeds)
        {
            float fulfillment = need.GetFulfillment(this);
            if (need == marginal_need)
                fulfillment += marginal_need.GetMarginalFulfillment(this, credits);

            float need_growth_rate_modifier = 
                need.FulfillmentToGrowthRateModifier(fulfillment);

            if (need_growth_rate_modifier > 1)
                growth_rate += need_growth_rate_modifier - 1;
            else if (need_growth_rate_modifier > 0)
                growth_rate_modifier *= need_growth_rate_modifier;
            else
            {
                death_rate = 1 - (1 - death_rate) *
                                 (1 + need_growth_rate_modifier);
                growth_rate_modifier = 0;
            }
        }


        float growth_rate_per_meeting = 
            Mathf.Pow(1 + growth_rate * growth_rate_modifier, 
                      1.0f / ShoppingTrip.SessionsPerYear) - 1;

        float death_rate_per_meeting = 
            1 - Mathf.Pow(1 - death_rate, 1.0f / ShoppingTrip.SessionsPerYear);

        return Population *
               (growth_rate_per_meeting  - death_rate_per_meeting);
    }

    public class Need
    {
        public System.Func<People, float> GetFulfillment;
        public System.Func<float, float> FulfillmentToGrowthRateModifier;

        public float GetGrowthRate(People people, float fulfillment = -1)
        {
            if (fulfillment < 0)
                fulfillment = GetFulfillment(people);

            return FulfillmentToGrowthRateModifier(fulfillment);
        }
    }

    public class PurchasableNeed : Need
    {
        public System.Func<People, float, float> GetMarginalFulfillment;

        public float GetMarginalUtilityOfPurchase(People people, float credits)
        {
            return people.GetPopulationChange(this, credits) - 
                   people.GetPopulationChange();
        }
    }

    [System.Serializable]
    public class ItemNeed : PurchasableNeed
    {
        public string ItemName;

        public float MinimumConsumption;
        public float HealthyConsumption;
        public float MaximumConsumption;
        public float MaximumDeathRate;
        public float MaximumGrowthRate;

        public ItemNeed()
        {
            GetFulfillment = 
                people => people.Storage.GetQuantity(ItemName) / 
                          (people.Population * people.ShoppingTrip.DaysBetweenSessions);

            GetMarginalFulfillment = 
                (people, credits) => 
                people.Item.Station().OfficialMarket
                    .GetPurchaseQuantity(ItemName, credits) /
                    (people.Population * people.ShoppingTrip.DaysBetweenSessions);

            FulfillmentToGrowthRateModifier = 
            delegate (float quantity)
            {
                Dictionary<float, float> growth_rate_lookup =
                    new Dictionary<float, float>();
                growth_rate_lookup[0] = -MaximumDeathRate;
                growth_rate_lookup[MinimumConsumption] = 0;
                growth_rate_lookup[HealthyConsumption] = 1;
                growth_rate_lookup[MaximumConsumption] = 1 + MaximumGrowthRate;

                System.Func<float, float> GrowthRateFunction = 
                    MathUtility.LookupToFunction_LinearInterpolation(growth_rate_lookup);

                return GrowthRateFunction(quantity);
            };
        }
    }
}
