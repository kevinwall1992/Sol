using UnityEngine;
using System.Collections;
using System.Linq;

[System.Serializable]
public class SaleOffer
{
    public User Seller;

    public Item Item;

    public float CostPerUnit;

    public SaleOffer(User seller, Item item, float cost_per_unit)
    {
        Seller = seller;
        Item = item;
        CostPerUnit = cost_per_unit;
    }

    public SaleOffer() { }

    public Item Transact(User buyer, float quantity)
    {
        if (quantity == 0)
            return null;

        quantity = Mathf.Min(Item.Quantity, quantity);

        Item purchased_item = Item.RemoveQuantity(quantity);

        Seller.PrimaryBankAccount.Deposit(
            buyer.PrimaryBankAccount.Withdraw(quantity * CostPerUnit));
        purchased_item.Owner = buyer;

        return purchased_item;
    }
}

[System.Serializable]
public class PurchaseOffer
{
    public User Buyer;
    public Storage Destination;

    public string ItemName;
    public float Quantity;

    public float ValuePerUnit;

    public PurchaseOffer(User buyer, 
                         Storage destination, 
                         string item_name, 
                         float quantity, 
                         float cost_per_unit)
    {
        Buyer = buyer;
        Destination = destination;
        ItemName = item_name;
        Quantity = quantity;
        ValuePerUnit = cost_per_unit;
    }

    public PurchaseOffer() { }

    public Item Transact(User seller, Item item)
    {
        if (item.Name != ItemName)
            return item;

        Item purchased_item = item.RemoveQuantity(Quantity);
        Quantity -= purchased_item.Quantity;

        seller.PrimaryBankAccount.Deposit(
            Buyer.PrimaryBankAccount.Withdraw(purchased_item.Quantity * ValuePerUnit));
        purchased_item.Owner = Buyer;

        if (Destination != null)
            Destination.Store(purchased_item);

        //Temporary hack for testing purposes
        else if (Buyer.HasComponent<StationManagement>())
            Buyer.GetComponent<StationManagement>().Stations.First().Craft.Cargo.Store(purchased_item);

        return item;
    }
}
