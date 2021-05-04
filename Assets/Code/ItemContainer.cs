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
    { get { return Items.Sum(item => item.Volume()); } }

    public float AvailableVolume
    { get { return Volume - ItemVolume; } }

    //Mixed ownership is not yet supported, so an .Owner field is necessary. 
    public User Owner;

    public IEnumerable<Script> Scripts { get { return GetComponents<Script>(); } }
    public IEnumerable<Filter> Filters { get { return GetComponents<Filter>(); } }
    public IEnumerable<Packer> Packers { get { return GetComponents<Packer>(); } }

    public IEnumerable<Item> Items
    { get { return transform.Children().SelectComponents<Item>(); } }

    public Manifest Manifest { get { return Items.ToManifest(); } }

    public float ItemMass
    {
        get
        {
            float total = 0;

            foreach (Item item in Items)
                total += item.Mass() * item.Quantity;

            return total;
        }
    }

    public Craft Craft { get { return Item.Craft(); } }

    private void Start()
    {
        MergeDuplicates();

        foreach (Item item in Items)
            item.TakeSample();
    }

    void MergeDuplicates()
    {
        ItemSet items = new ItemSet();

        foreach (Item item in GetComponentsInChildren<Item>().ToList())
        {
            if (!items.Contains(item))
                items.Add(item);
            else
            {
                items.Get(item).Quantity += item.Quantity;
                Destroy(item.gameObject);
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

    public bool IsStorable(Item example)
    {
        foreach (Filter filter in Filters)
            if (!filter.IsStorable(example))
                return false;

        return true;
    }

    public Item GetItem(Item example)
    {
        return Items.FirstOrDefault(item => item.IsEquivalent(example));
    }

    public float GetQuantity(Item example)
    {
        if (example == null)
            return 0;

        return Items
            .Where(item => item.IsEquivalent(example))
            .Sum(item => item.Quantity);
    }

    public bool Contains(Item example)
    {
        return GetQuantity(example) > 0;
    }

    public bool PutIn(Item item)
    {
        item.TakeSample();

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

        item.transform.SetParent(transform);
        MergeDuplicates();

        return true;
    }


    //Increase quantity of an object without requiring callers
    //to create Item GameObjects.
    //In the future, may be used to cut down on Instantiate() calls. 

    public float PutInQuantity(Item example, float quantity)
    {
        if (!IsStorable(example))
            return quantity;

        Item item = example.Copy();
        item.Quantity = quantity;

        if(!PutIn(item))
        {
            float overflow = item.Quantity;
            GameObject.Destroy(item);

            return overflow;
        }

        return 0;
    }

    public Item TakeOut(Item example, float quantity = -1)
    {
        if (!Contains(example))
            return null;

        Item removed_item = GetItem(example).RemoveQuantity(quantity);

        Unpack(removed_item);

        return removed_item;
    }

    public float TakeOutQuantity(Item example, float quantity)
    {
        if (!Contains(example))
            return 0;

        Item item = GetItem(example);

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
