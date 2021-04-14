using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class Manifest
{
    Dictionary<string, float> quantities = 
        new Dictionary<string, float>();
    Dictionary<string, Item> items = 
        new Dictionary<string, Item>();

    public IEnumerable<Item> Items
    { get { return items.Values; } }

    public float this[Item item]
    {
        get { return GetQuantity(item); }
        set { Add(item, value); }
    }
    public float this[string item_name]
    { get { return GetQuantity(item_name); } }


    public Manifest(Dictionary<Item, float> quantities)
    {
        foreach (Item item in quantities.Keys)
            Add(item, quantities[item]);
    }

    public Manifest()
    {

    }

    public void Add(Item item, float quantity)
    {
        if (!items.ContainsKey(item.Name))
            items[item.Name] = item;

        if (!quantities.Keys.Contains(item.Name))
            quantities[item.Name] = 0;

        quantities[item.Name] += quantity;
    }

    public void Add(Manifest other)
    {
        foreach (Item item in other.Items)
            Add(item, other.GetQuantity(item));
    }

    public bool Contains(Item item)
    {
        return Contains(item.Name);
    }

    public bool Contains(string item_name)
    {
        return GetQuantity(item_name) > 0;
    }

    public float GetQuantity(Item item)
    {
        return GetQuantity(item.Name);
    }

    public float GetQuantity(string item_name)
    {
        if (!quantities.ContainsKey(item_name))
            return 0;

        return quantities[item_name];
    }

    public void Scale(float scalar)
    {
        foreach (string item_name in quantities.Keys.ToList())
            quantities[item_name] *= scalar;
    }

    public Manifest Collated(Manifest other)
    {
        Manifest combined = new Manifest();
        combined.Add(this);
        combined.Add(other);

        return combined;
    }
}
