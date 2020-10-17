using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Market : MonoBehaviour
{
    public Station Station;

    public List<PurchaseOffer> PurchaseOffers = new List<PurchaseOffer>();
    public List<SaleOffer> SaleOffers = new List<SaleOffer>();

    private void Update()
    {
        
    }

    public PurchaseOffer PostOffer(PurchaseOffer purchase_offer)
    {
        PurchaseOffers.Add(purchase_offer);

        return purchase_offer;
    }

    public SaleOffer PostOffer(SaleOffer sale_offer)
    {
        SaleOffers.Add(sale_offer);

        return sale_offer;
    }

    public PurchaseOffer RescindOffer(PurchaseOffer purchase_offer)
    {
        if (PurchaseOffers.Contains(purchase_offer))
            PurchaseOffers.Remove(purchase_offer);

        return purchase_offer;
    }

    public SaleOffer RescindOffer(SaleOffer sale_offer)
    {
        if (!SaleOffers.Contains(sale_offer))
            SaleOffers.Remove(sale_offer);

        return sale_offer;
    }

    public List<SaleOffer> GetSortedSaleOffersFor(string item_name)
    {
        return SaleOffers
            .Where(offer => offer.Item.Name == item_name)
            .Sorted(offer => offer.CostPerUnit);
    }

    public List<PurchaseOffer> GetSortedPurchaseOffersFor(string item_name)
    {
        return PurchaseOffers
            .Where(offer => offer.ItemName == item_name)
            .Sorted(offer => offer.ValuePerUnit).Reversed();
    }

    public IEnumerable<PurchaseOffer> GetPurchaseOffersBy(User buyer)
    {
        return PurchaseOffers.Where(offer => offer.Buyer == buyer);
    }

    public IEnumerable<SaleOffer> GetSaleOffersBy(User seller)
    {
        return SaleOffers.Where(offer => offer.Seller == seller);
    }

    public Item GetSampleItem(string item_name)
    {
        return SaleOffers
            .Select(offer => offer.Item)
            .FirstOrDefault(item => item.Name == item_name);
    }

    public IEnumerable<Item> GetSampleItems()
    {
        return SaleOffers
            .Select(offer => offer.Item.Name)
            .RemoveDuplicates()
            .Select(item_name => GetSampleItem(item_name));
    }

    public IEnumerable<T> GetSampleItems<T>() where T : Item.Script
    {
        return GetSampleItems().SelectComponents<Item, T>();
    }

    public float GetQuantity(string item_name)
    {
        return SaleOffers.Sum(offer => offer.Item.Quantity);
    }

    public bool IsAvailable(string item_name)
    {
        return GetQuantity(item_name) > 0;
    }

    public float GetPurchaseCost(string item_name, float quantity)
    {
        if (GetQuantity(item_name) < quantity)
            return float.MaxValue;

        float cost = 0;

        foreach (SaleOffer sale_offer in GetSortedSaleOffersFor(item_name))
        {
            float quantity_from_this_offer = 
                Mathf.Min(quantity, sale_offer.Item.Quantity);

            cost += sale_offer.CostPerUnit * quantity_from_this_offer;
            quantity -= quantity_from_this_offer;

            if (quantity == 0)
                break;
        }

        return cost;
    }

    public float GetPurchaseCostPerUnit(string item_name, float quantity)
    {
        return GetPurchaseCost(item_name, quantity) / quantity;
    }

    public float GetPurchaseQuantity(string item_name, float credits)
    {
        float total_quantity = 0;

        foreach(SaleOffer sale_offer in GetSortedSaleOffersFor(item_name))
        {
            float quantity = Mathf.Min(sale_offer.Item.Quantity, 
                                       credits / sale_offer.CostPerUnit);

            total_quantity += quantity;
            credits -= quantity * sale_offer.CostPerUnit;

            if (credits == 0)
                break;
        }

        return total_quantity;
    }

    public float GetSaleValue(string item_name, float quantity)
    {
        float sale_value = 0;

        foreach (PurchaseOffer purchase_offer in GetSortedPurchaseOffersFor(item_name))
        {
            float quantity_from_this_offer =
                Mathf.Min(quantity, purchase_offer.Quantity);

            sale_value += purchase_offer.ValuePerUnit * quantity_from_this_offer;
            quantity -= quantity_from_this_offer;

            if (quantity == 0)
                break;
        }

        return sale_value;
    }

    public float GetSaleValuePerUnit(string item_name, float quantity)
    {
        return GetSaleValue(item_name, quantity) / quantity;
    }

    public Item Purchase(User buyer, string item_name, float quantity, Storage storage)
    {
        if (quantity > GetQuantity(item_name) || 
            GetPurchaseCost(item_name, quantity) > buyer.PrimaryBankAccount.Balance)
            return null;

        Item item_purchased = GetSampleItem(item_name).Copy();
        item_purchased.Quantity = 0;

        if (quantity > storage.GetUnusedVolumeFor(item_purchased))
            return null;

        foreach (SaleOffer offer in GetSortedSaleOffersFor(item_name))
        {
            Item transaction_item = offer.Transact(buyer, quantity);

            item_purchased.Quantity += transaction_item.Quantity;
            quantity -= transaction_item.Quantity;
            GameObject.Destroy(transaction_item.gameObject);

            if (offer.Item.Quantity == 0)
            {
                GameObject.Destroy(offer.Item.gameObject);
                SaleOffers.Remove(offer);
            }

            if (quantity == 0)
                break;
        }

        storage.Store(item_purchased);

        return item_purchased;
    }

    public Item Sell(User seller, Item item)
    {
        foreach (PurchaseOffer offer in GetSortedPurchaseOffersFor(item.Name))
        {
            offer.Transact(seller, item);

            if (offer.Quantity == 0)
                PurchaseOffers.Remove(offer);

            if (item.Quantity == 0)
                break;
        }

        return item;
    }
}
