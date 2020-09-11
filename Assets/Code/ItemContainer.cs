using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ExecuteAlways]
public class ItemContainer : MonoBehaviour, Craft.Part
{
    public float DryMass;
    public float Volume;

    public float PartMass { get { return DryMass; } }

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

            return GetComponentsInChildren<Item>()
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

    public Craft Craft { get { return GetComponentInParent<Craft>(); } }

    private void Update()
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

    public Item PutIn(Item item)
    {
        if (AvailableVolume == 0)
            return item;

        Pack(item);

        if (!IsStorable(item))
        {
            Unpack(item);
            return item;
        }

        Item overflow = null;
        if (AvailableVolume < item.Volume())
            overflow = item.RemoveVolume(item.Volume() - AvailableVolume);

        item.transform.SetParent(transform);
        Items[item.Name] = item;

        return overflow;
    }

    public Item TakeOut(string name, float quantity = -1)
    {
        if (!Items.ContainsKey(name))
            return null;

        Item item;

        if (quantity >= Items[name].Quantity)
            item = Items[name];
        else
        {
            item = GameObject.Instantiate(Items[name]);

            Items[name].Quantity -= item.Quantity = quantity;
        }

        Unpack(item);
        return item;
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
