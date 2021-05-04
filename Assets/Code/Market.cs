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
    { get { return SaleOffers.Select(offer => offer.Sample).Distinct(); } }

    public Manifest Manifest
    {
        get
        {
            return new Manifest(
                Wares.Select(sample => (sample, GetTotalSupply(sample))));
        }
    }

    private void Awake()
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

    public List<SaleOffer> GetSortedSaleOffersFor(Item example)
    {
        return SaleOffers
            .Where(offer => offer.Sample.IsEquivalent(example))
            .Sorted(offer => offer.CostPerUnit);
    }

    public List<PurchaseOffer> GetSortedPurchaseOffersFor(Item example)
    {
        return PurchaseOffers
            .Where(offer => offer.Sample.IsEquivalent(example))
            .Sorted(offer => offer.ValuePerUnit).Reversed();
    }

    public IEnumerable<PurchaseOffer> GetPurchaseOffersBy(User buyer)
    {
        return PurchaseOffers.Where(offer => offer.Buyer == buyer);
    }

    public IEnumerable<PurchaseOffer> GetPurchaseOffersBy(User buyer, Item example)
    {
        return PurchaseOffers.Where(offer => offer.Buyer == buyer &&
                                    offer.Sample.IsEquivalent(example));
    }

    public IEnumerable<SaleOffer> GetSaleOffersBy(User seller)
    {
        return SaleOffers.Where(offer => offer.Seller == seller);
    }

    public IEnumerable<SaleOffer> GetSaleOffersBy(User seller, Item example)
    {
        return SaleOffers.Where(offer => offer.Seller == seller &&
                                offer.Sample.IsEquivalent(example));
    }

    public float GetTotalSupply(Item example)
    {
        if (example == null)
            return 0;

        return SaleOffers
            .Where(offer => offer.Sample.IsEquivalent(example))
            .Sum(offer => offer.AvailableSupply);
    }

    public bool IsAvailable(Item example)
    {
        return GetTotalSupply(example) > 0;
    }

    public float GetPurchaseCost(Item example, float quantity)
    {
        if (GetTotalSupply(example) < quantity)
            return float.MaxValue;

        float cost = 0;

        foreach (SaleOffer sale_offer in GetSortedSaleOffersFor(example))
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

    public float GetGoingPrice(Item example)
    {
        return GetPurchaseCost(example, 0.001f) / 0.001f;
    }

    public float GetPurchaseCostPerUnit(Item example, float quantity)
    {
        return GetPurchaseCost(example, quantity) / quantity;
    }

    public float GetPurchaseQuantity(Item example, float credits)
    {
        if (example == null)
            return 0;

        float total_quantity = 0;

        foreach (SaleOffer sale_offer in GetSortedSaleOffersFor(example))
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

    public float GetSaleValue(Item example, float quantity)
    {
        float sale_value = 0;

        foreach (PurchaseOffer purchase_offer in GetSortedPurchaseOffersFor(example))
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

    public float GetSaleValuePerUnit(Item example, float quantity)
    {
        return GetSaleValue(example, quantity) / quantity;
    }

    public float GetTotalDemand(Item example)
    {
        if (example == null)
            return 0;

        return PurchaseOffers
            .Where(offer => offer.Sample.IsEquivalent(example))
            .Sum(offer => offer.AvailableDemand);
    }

    //The following two methods are all-or-nothing. 
    public bool Purchase(User buyer, Storage destination,
                         Item example, float quantity)
    {
        if (quantity > GetTotalSupply(example) ||
            quantity > GetTotalSupply(example))
            return false;

        if (quantity * example.GetVolumePerUnit() >
            destination.GetUnusedVolumeFor(example))
            return false;

        foreach (SaleOffer offer in GetSortedSaleOffersFor(example))
        {
            if (quantity <= 0)
                break;

            quantity -= offer.Transact(buyer, destination, quantity);
        }

        return true;
    }

    public bool Sell(User seller, Storage source,
                     Item example, float quantity)
    {
        if (quantity > source.GetQuantity(example))
            return false;
        if (quantity > GetTotalDemand(example))
            return false;

        foreach (PurchaseOffer offer in GetSortedPurchaseOffersFor(example))
        {
            if (quantity <= 0)
                break;

            quantity -= offer.Transact(seller, source, quantity);
        }

        return true;
    }
}