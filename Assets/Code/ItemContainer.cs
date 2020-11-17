using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class ItemContainer : Craft.Part
{
    public float VolumePerUnit;

    public float Volume { get { return VolumePerUnit * Item.Quantity; } }

    public float TotalMass { get { return PartMass + ItemMass; } }

    public float ItemVolume
    { get { return Items.Values.Sum(item => item.Volume()); } }

    public float AvailableVolume
    { get { return Volume - ItemVolume; } }

    public IEnumerable<Script> Scripts { get { return GetComponents<Script>(); } }
    public IEnumerable<Filter> Filters { get { return GetComponents<Filter>(); } }
    public IEnumerable<Packer> Packers { get { return GetComponents<Packer>(); } }

    public Dictionary<string, Item> Items
    {
        get
        {
            MergeDuplicates();

            return transform.Children()
                .SelectComponents<Item>()
                .ToDictionary(item => item.Name, item => item);
        }
    }

    public float ItemMass
    {
        get
        {
            float total = 0;

            foreach (Item item in Items.Values)
                total += item.Mass();

            return total;
        }
    }

    public Craft Craft { get { return Item.Craft(); } }

    protected void Update()
    {
        MergeDuplicates();
    }

    void MergeDuplicates()
    {
        Dictionary<string, Item> items = new Dictionary<string, Item>();

        foreach (Item item in GetComponentsInChildren<Item>().ToList())
        {
            if (!items.ContainsKey(item.Name))
                items[item.Name] = item;
            else
            {
                items[item.Name].Quantity += item.Quantity;
                GameObject.Destroy(item.gameObject);
            }
        }
    }

    public void Pack(Item item)
    {
        foreach (Packer packer in Packers)
            packer.Pack(item);
    }

    public void Unpack(Item item)
    {
        foreach (Packer packer in Packers)
            packer.Unpack(item);
    }

    public bool IsStorable(Item item)
    {
        foreach (Filter filter in Filters)
            if (!filter.IsStorable(item))
                return false;

        return true;
    }

    public Item GetItem(string name)
    {
        if (!Items.ContainsKey(name))
            return null;

        return Items[name];
    }

    public float GetQuantity(string name)
    {
        Item item = GetItem(name);
        if (item == null)
            return 0;

        return item.Quantity;
    }

    public bool Contains(string name)
    {
        return GetQuantity(name) > 0;
    }

    public bool ContainsSpecificItem(Item item)
    {
        return Items.Values.Contains(item);
    }

    public bool PutIn(Item item)
    {
        if (AvailableVolume == 0)
            return false;

        Pack(item);

        if (!IsStorable(item))
        {
            Unpack(item);
            return false;
        }

        if (AvailableVolume < item.Volume())
        {
            PutIn(item.RemoveVolume(item.Volume() - AvailableVolume));

            Unpack(item);
            return false;
        }

        if (!Items.ContainsKey(item.Name))
        {
            item.transform.SetParent(transform);
            Items[item.Name] = item;
        }
        else
        {
            Items[item.Name].Quantity += item.Quantity;
            GameObject.Destroy(item.gameObject);
        }

        return true;
    }

    //Increase quantity of an object without requiring callers
    //to create Item GameObjects.
    //In the future, may be used to cut down on Instantiate() calls. 
    public float PutInQuantity(Item sample_item, float quantity)
    {
        if (!IsStorable(sample_item))
            return quantity;

        Item item = sample_item.Copy();
        item.Quantity = quantity;

        if(!PutIn(item))
        {
            float overflow = item.Quantity;

            GameObject.Destroy(item);
            return overflow;
        }

        return 0;
    }

    public Item TakeOut(string name, float quantity = -1)
    {
        if (!Items.ContainsKey(name))
            return null;

        Item removed_item = GetItem(name).RemoveQuantity(quantity);

        Unpack(removed_item);

        return removed_item;
    }

    public float TakeOutQuantity(string name, float quantity)
    {
        if (!Items.ContainsKey(name))
            return 0;

        Item item = GetItem(name);

        quantity = Mathf.Min(quantity, item.Quantity);

        item.Quantity -= quantity;
        return quantity;
    }


    [RequireComponent(typeof(ItemContainer))]
    public abstract class Script : MonoBehaviour
    {
        public ItemContainer Container
        { get { return GetComponent<ItemContainer>(); } }
    }

    public interface Filter
    {
        bool IsStorable(Item item);
    }

    public interface Packer
    {
        void Pack(Item item);
        void Unpack(Item item);
    }
}
