using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Market : MonoBehaviour
{
    public Station Station;

    public List<PurchaseOffer> PurchaseOffers = new List<PurchaseOffer>();
    public List<SaleOffer> SaleOffers = new List<SaleOffer>();

    public IEnumerable<Item> Wares
    {
        get
        {
            return SaleOffers
                .Where(offer => offer.AvailableSupply > 0)
                .GroupBy(offer => offer.Sample.Name)
                .Select(group => group.First().Sample);
        }
    }

    private void Start()
    {
        //This a hack until we get rid of in-editor offers
        foreach (PurchaseOffer offer in PurchaseOffers)
            if (offer.Destination == null)
                offer.Destination = Station.GetStorage(offer.Buyer);

        foreach (SaleOffer offer in SaleOffers)
            if (offer.Source == null)
                offer.Source = Station.GetStorage(offer.Seller);
    }

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
            .Where(offer => offer.Sample.Name == item_name)
            .Sorted(offer => offer.CostPerUnit);
    }

    public List<PurchaseOffer> GetSortedPurchaseOffersFor(string item_name)
    {
        return PurchaseOffers
            .Where(offer => offer.Sample.Name == item_name)
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

    public float GetTotalSupply(string item_name)
    {
        return SaleOffers
            .Where(offer => offer.Sample.Name == item_name)
            .Sum(offer => offer.AvailableSupply);
    }

    public bool IsAvailable(string item_name)
    {
        return GetTotalSupply(item_name) > 0;
    }

    public float GetPurchaseCost(string item_name, float quantity)
    {
        if (GetTotalSupply(item_name) < quantity)
            return float.MaxValue;

        float cost = 0;

        foreach (SaleOffer sale_offer in GetSortedSaleOffersFor(item_name))
        {
            float quantity_from_this_offer = 
                Mathf.Min(quantity, sale_offer.AvailableSupply);

            cost += sale_offer.CostPerUnit * quantity_from_this_offer;
            quantity -= quantity_from_this_offer;

            if (quantity <= 0)
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
            float quantity = Mathf.Min(sale_offer.AvailableSupply, 
                                       credits / sale_offer.CostPerUnit);

            total_quantity += quantity;
            credits -= quantity * sale_offer.CostPerUnit;

            if (credits <= 0)
                break;
        }

        return total_quantity;
    }

    public float GetSaleValue(string item_name, float quantity)
    {
        float sale_value = 0;

        foreach (PurchaseOffer purchase_offer in GetSortedPurchaseOffersFor(item_name))
        {
            float quantity_from_this_offer = Mathf.Min(
                quantity, 
                purchase_offer.AvailableDemand);

            sale_value += purchase_offer.ValuePerUnit * quantity_from_this_offer;
            quantity -= quantity_from_this_offer;

            if (quantity <= 0)
                break;
        }

        return sale_value;
    }

    public float GetSaleValuePerUnit(string item_name, float quantity)
    {
        return GetSaleValue(item_name, quantity) / quantity;
    }

    public float GetTotalDemand(string item_name)
    {
        return PurchaseOffers
            .Where(offer => offer.Sample.Name == item_name)
            .Sum(offer => offer.AvailableDemand);
    }

    //The following two methods are all-or-nothing. 
    public bool Purchase(User buyer, Storage destination, 
                         string item_name, float quantity)
    {
        if (quantity > GetTotalSupply(item_name) || 
            quantity > GetTotalSupply(item_name))
            return false;

        Item sample_item = 
            SaleOffers.First(offer => offer.Sample.Name == item_name).Sample;

        if (quantity * sample_item.Physical().VolumePerUnit > 
            destination.GetUnusedVolumeFor(sample_item))
            return false;

        foreach (SaleOffer offer in GetSortedSaleOffersFor(item_name))
        {
            if (quantity <= 0)
                break;

            quantity -= offer.Transact(buyer, destination, quantity);
        }

        return true;
    }

    public bool Sell(User seller, Storage source, 
                     string item_name, float quantity)
    {
        if (quantity > source.GetQuantity(item_name))
            return false;
        if (quantity > GetTotalDemand(item_name))
            return false;

        foreach (PurchaseOffer offer in GetSortedPurchaseOffersFor(item_name))
        {
            if (quantity <= 0)
                break;

            quantity -= offer.Transact(seller, source, quantity);
        }

        return true;
    }
}
