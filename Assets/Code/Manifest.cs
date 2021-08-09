using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Manifest
{
    public Dictionary<Item, float> quantities =
        new Dictionary<Item, float>(Item.EquivalencyComparer);

    public IEnumerable<Item> Samples
    { get { return quantities.Keys; } }

    public float this[Item item]
    {
        get { return GetQuantity(item); }
        set { Add(item, value); }
    }

    public Manifest(Dictionary<Item, float> quantities_)
    {
        foreach (Item item in quantities_.Keys)
            Add(item, quantities_[item]);
    }

    public Manifest(IEnumerable<(Item Item, float Quantity)> items)
    {
        foreach (var pair in items)
            Add(pair.Item, pair.Quantity);
    }

    public Manifest(IEnumerable<Item> items)
    {
        foreach (Item item in items)
            Add(item);
    }

    public Manifest()
    {

    }

    public void Add(Item item, float quantity)
    {
        if (!quantities.Keys.Contains(item))
            quantities[item.TakeSample()] = 0;

        quantities[item] += quantity;
    }

    public void Add(Item item)
    {
        Add(item, item.Quantity);
    }

    public void Add(Manifest other)
    {
        foreach (Item item in other.Samples)
            Add(item, other.GetQuantity(item));
    }

    public float Remove(Item item, float quantity = -1)
    {
        if (quantity < 0)
            quantity = GetQuantity(item);
        else if (GetQuantity(item) < quantity)
            return 0;

        quantities[item] -= quantity;

        return quantity;
    }

    public bool Contains(Item item)
    {
        return GetQuantity(item) > 0;
    }

    public float GetQuantity(Item item)
    {
        if (!quantities.ContainsKey(item))
            return 0;

        return quantities[item];
    }

    public void Scale(float scalar)
    {
        foreach (Item item in quantities.Keys.ToList())
            quantities[item] *= scalar;
    }

    public Manifest Collated(Manifest other)
    {
        Manifest combined = new Manifest();
        combined.Add(this);
        combined.Add(other);

        return combined;
    }
}


public static class ManifestExtensions
{
    public static Manifest ToManifest(this IEnumerable<Item> items)
    {
        return new Manifest(items);
    }
}
