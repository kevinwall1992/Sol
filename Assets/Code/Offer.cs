using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

[System.Serializable]
public class SaleOffer
{
    public User Seller;
    public Inventory Source;

    public Item Sample;

    public float OfferedSupply;
    public float AvailableSupply
    { get { return Mathf.Min(Source.GetQuantity(Sample), OfferedSupply); } }

    public float CostPerUnit;


    public SaleOffer(User seller, Inventory source,
                     Item example, float quantity, float cost_per_unit)
    {
        Seller = seller;
        Source = source;

        Sample = example.TakeSample();
        OfferedSupply = quantity;
        CostPerUnit = cost_per_unit;
    }

    public SaleOffer() { }

    public bool Transact(User buyer, Inventory destination, float quantity)
    {
        if (quantity > OfferedSupply ||
            quantity > AvailableSupply)
            return false;

        Source.TakeOut(Sample, quantity);
        OfferedSupply -= quantity;

        destination.PutIn(Sample, quantity);

        Seller.PrimaryBankAccount.Deposit(
            buyer.PrimaryBankAccount.Withdraw(quantity * CostPerUnit));

        return true;
    }
}

[System.Serializable]
public class PurchaseOffer
{
    public User Buyer;
    public Inventory Destination;

    public Item Sample;

    public float OfferedDemand;
    public float AvailableDemand
    {
        get
        {
            return Mathf.Min(OfferedDemand,
                             Destination.GetSpaceAvailable(Sample));
        }
    }

    public float ValuePerUnit;

    public PurchaseOffer(User buyer,
                         Inventory destination,
                         Item example,
                         float quantity,
                         float cost_per_unit)
    {
        Buyer = buyer;
        Destination = destination;
        Sample = example.TakeSample();
        OfferedDemand = quantity;
        ValuePerUnit = cost_per_unit;
    }

    public PurchaseOffer() { }

    public bool Transact(User seller, Inventory source,
                         float quantity)
    {
        if (quantity > OfferedDemand ||
            quantity > AvailableDemand)
            return false;

        source.TakeOut(Sample, quantity);

        OfferedDemand -= quantity;

        //Temporary hack for testing purposes
        if (Destination != null)
            Destination.PutIn(Sample, quantity);
        else if (Buyer.HasComponent<StationManagement>())
            Buyer.GetComponent<StationManagement>().Stations.First().Craft.Cargo.PutIn(Sample, quantity);

        seller.PrimaryBankAccount.Deposit(
            Buyer.PrimaryBankAccount.Withdraw(quantity * ValuePerUnit));

        return true;
    }
}
