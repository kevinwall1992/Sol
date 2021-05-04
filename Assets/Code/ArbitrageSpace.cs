using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;


public partial class Arbitrage
{
    public static BudgetSpace CreateBudgetSpace(
        float budget,
        Market market,
        Func<Item, float, float> GetTransportCosts = null)
    {
        if (GetTransportCosts == null)
            GetTransportCosts = (item, quantity) => 0;

        Func<Item, float, float> GetVariableCosts = (item, quantity) =>
        {
            return market.GetPurchaseCost(item, quantity) +
                   GetTransportCosts(item, quantity);
        };

        return new BudgetSpace(
            budget,
            GetVariableCosts,
            (item, credits) => market.GetPurchasableQuantity(item, credits));

    }
    public static CompoundSpace CreateStorageSpace(
        Storage storage, 
        Manifest items = null)
    {   
        CompoundSpace space = 
            new CompoundSpace(storage.GetItemContainers()
            .Select(item_container => 
                new LinearSpace(item_container.Volume,
                                item => item.Physical().VolumePerUnit,
                                item_container.IsStorable)));

        if(items != null)
            foreach (Item item in items.Samples)
                space.Pack(item, items[item]);

        return space;
    }


    public abstract class Space
    {
        public abstract float GetCapacity(Item item);
        public abstract void Pack(Item item, float quantity);


        //This is an estimate of what fraction of the remaining volume a given
        //quantity of an item will take up. It will only be perfectly accurate 
        //if the space is linear.

        //Derived types may override this to give a better estimate.

        public virtual float GetCapacityFraction(Item item, float quantity)
        {
            return quantity / GetCapacity(item);
        }


        //Finds largest quantity of item such that the ratio of quantity to
        //"volume" is roughly linear - in other words the first tiny amount's
        //ratio is able to predict the volume of the whole marginal quantity.

        //General algorithm: 
        //Guess marginal quantity is == GetCapacity() by assuming linearity. 
        //If guess was wrong, then guess again by assuming the derivative is 
        //linear and using this to determine the point where the error == 
        //error_tolerance. 
        //Because this process would never complete in some cases where the 
        //derivative is nonlinear, (f.e. monotonically increasing derivative 
        //means you just keep geting X% closer but never arriving), there is 
        //a "ZenoFraction" which makes our guess more conservative - 
        //essentially, crossing that extra space. 
        //Non-monotonic spaces may result in a false positive in terms of 
        //detecting linearity. This is a probabilistic and is proportional to 
        //the size of the error tolerance. 

        const float EpsilonQuantity = 0.001f;
        const float ErrorTolerance = 0.1f;
        const float ZenoFraction = 0.9f;
        public virtual float GetMarginalQuantity(Item item)
        {
            float epsilon_ratio = GetCapacityFraction(item, EpsilonQuantity) / 
                                  EpsilonQuantity;
            float marginal_quantity = GetCapacity(item);

            Func<float> GetError = () =>
            {
                float marginal_ratio = 
                    GetCapacityFraction(item, marginal_quantity) / 
                    marginal_quantity;

                return Mathf.Abs(marginal_ratio - epsilon_ratio) /
                       epsilon_ratio;
            };

            while (GetError() > ErrorTolerance)
                marginal_quantity *= ZenoFraction *
                                     ErrorTolerance /
                                     GetError();

            return marginal_quantity;
        }
    }

    public class BudgetSpace : Space
    {
        public float Budget { get; }
        public float Expenditure { get; private set; }

        public Func<Item, float, float> GetVariableCosts { get; }
        public Func<Item, float, float> GetPurchasableQuantity { get; }

        public Manifest Purchases { get; } = new Manifest();

        public float RemainingFunds
        { get { return Budget - Expenditure; } }

        public BudgetSpace(float budget,
                           Func<Item, float, float> GetVariableCosts_,
                           Func<Item, float, float> GetPurchasableQuantity_)
        {
            Budget = budget;
            Expenditure = 0;

            GetVariableCosts = GetVariableCosts_;
            GetPurchasableQuantity = GetPurchasableQuantity_;
        }

        public override float GetCapacity(Item item)
        {
            return GetPurchasableQuantity(item, RemainingFunds);
        }

        public override void Pack(Item item, float quantity)
        {
            Expenditure += GetVariableCosts(item, Purchases[item] + quantity) -
                           GetVariableCosts(item, Purchases[item]);

            Purchases[item] += quantity;
        }

        public override float GetCapacityFraction(Item item, float quantity)
        {
            return GetVariableCosts(item, quantity) / RemainingFunds;
        }
    }

    public class LinearSpace : Space
    {
        public float TotalVolume { get; }
        public float VolumeUsed { get; private set; }

        public Func<Item, float> Measure { get; }
        public Func<Item, bool> IsItemLegal { get; }

        public float VolumeRemaining
        { get { return TotalVolume - VolumeUsed; } }


        public LinearSpace(float total_volume,
                           Func<Item, float> Measure_,
                           Func<Item, bool> IsItemLegal_ = null,
                           float volume_used = 0)
        {
            TotalVolume = total_volume;
            VolumeUsed = volume_used;

            Measure = Measure_;

            if (IsItemLegal_ != null)
                IsItemLegal = IsItemLegal_;
            else
                IsItemLegal = item => true;
        }

        public override float GetCapacity(Item item)
        {
            if (!IsItemLegal(item))
                return 0;

            return VolumeRemaining / Measure(item);
        }

        public override void Pack(Item item, float quantity)
        {
            VolumeUsed += Measure(item) * quantity;
        }
    }

    public class CompoundSpace : Space
    {
        List<Space> subspaces;

        public CompoundSpace(IEnumerable<Space> subspaces_)
        {
            subspaces = subspaces_.ToList();
        }

        IEnumerable<Space> GetPackableSubspaces(Item item)
        {
            return subspaces
                .Where(subspace => subspace.GetCapacity(item) > 0);
        }

        public override float GetCapacity(Item item)
        {
            return GetPackableSubspaces(item)
                .Sum(subspace => subspace.GetCapacity(item));
        }

        public override void Pack(Item item, float quantity)
        {
            foreach (Space subspace in GetPackableSubspaces(item))
            {
                if (quantity == 0)
                    return;

                float packed_quantity =
                    Mathf.Min(quantity, subspace.GetCapacity(item));

                subspace.Pack(item, packed_quantity);
                quantity -= packed_quantity;
            }
        }
    }
}
