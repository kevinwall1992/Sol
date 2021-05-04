using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

[System.Serializable]
public class SaleOffer
{
    public User Seller;
    public Storage Source;

    public Item Sample;

    public float OfferedSupply;
    public float AvailableSupply
    { get { return Mathf.Min(Source.GetQuantity(Sample), OfferedSupply); } }

    public float CostPerUnit;


    public SaleOffer(User seller, Storage source,
                     Item example, float quantity, float cost_per_unit)
    {
        Seller = seller;
        Source = source;

        Sample = example.TakeSample();
        OfferedSupply = quantity;
        CostPerUnit = cost_per_unit;
    }

    public SaleOffer() { }

    public float Transact(User buyer, Storage destination, float quantity)
    {
        if (quantity > OfferedSupply ||
            quantity > AvailableSupply)
            return 0;

        Item item = Source.Retrieve(Sample, quantity);
        OfferedSupply -= item.Quantity;

        destination.Store(item);

        Seller.PrimaryBankAccount.Deposit(
            buyer.PrimaryBankAccount.Withdraw(item.Quantity * CostPerUnit));

        return item.Quantity;
    }
}

[System.Serializable]
public class PurchaseOffer
{
    public User Buyer;
    public Storage Destination;

    public Item Sample;

    public float OfferedDemand;
    public float AvailableDemand
    {
        get
        {
            return Mathf.Min(OfferedDemand,
                             Destination.GetMaximumVolumeOf(Sample));
        }
    }

    public float ValuePerUnit;

    public PurchaseOffer(User buyer,
                         Storage destination,
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

    public float Transact(User seller, Storage source,
                         float quantity)
    {
        if (quantity > OfferedDemand ||
            quantity > AvailableDemand)
            return 0;

        Item item = source.Retrieve(Sample, quantity);

        OfferedDemand -= item.Quantity;

        //Temporary hack for testing purposes
        if (Destination != null)
            Destination.Store(item);
        else if (Buyer.HasComponent<StationManagement>())
            Buyer.GetComponent<StationManagement>().Stations.First().Craft.Cargo.Store(item);

        seller.PrimaryBankAccount.Deposit(
            Buyer.PrimaryBankAccount.Withdraw(item.Quantity * ValuePerUnit));

        return item.Quantity;
    }
}
