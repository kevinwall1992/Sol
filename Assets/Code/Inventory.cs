using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;


public class Inventory : MonoBehaviour
{
    public IEnumerable<Item> Items
    { get { return transform.Children().SelectComponents<Transform, Item>(); } }

    public Manifest Manifest
    { get { return new Manifest(Items); } }

    public IEnumerable<Pocket> Pockets
    { get { return Items.SelectComponents<Item, Pocket>(); } }

    private void Update()
    {
        if (!Application.isEditor)
            return;

        foreach (Item item in Items)
            foreach (Item other_item in Items)
                if (item.IsEquivalent(other_item))
                    Debug.Assert(ReferenceEquals(item, other_item), 
                                 "Adding Item objects directly to Inventory is not supported. " +
                                 "Please use Inventory.Claim()");
    }

    public bool PutIn(Item example, float quantity = -1)
    {
        if (quantity < 0)
            quantity = example.Quantity;

        if (GetSpaceAvailable(example) < example.Size)
            return false;

        if (Manifest.Contains(example))
            GetItem(example).Quantity += quantity;
        else
        {
            if (example.IsFungible)
            {
                example = example.Copy();
                example.Quantity = quantity;
            }
            else
                Debug.Assert(quantity == example.Quantity);

            example.transform.SetParent(this.transform);
        }

        return true;
    }

    public void PutIn(Inventory other)
    {
        foreach(Item item in other.Items)
        {
            float quantity = other.GetQuantity(item);

            other.TakeOut(item, quantity);
            PutIn(item, quantity);
        }
    }

    public bool TakeOut(Item example, float quantity = -1)
    {
        if (GetQuantity(example) < quantity)
            return false;

        if (quantity < 0)
            quantity = GetQuantity(example);

        if (example.IsFungible)
            GetItem(example).Quantity -= quantity;
        else
        {
            Debug.Assert(quantity == GetQuantity(example));
            example.transform.SetParent(null);
        }

        return true;
    }

    public Item GetItem(Item example)
    {
        return GetComponentsInChildren<Item>()
            .FirstOrDefault(item => item.IsEquivalent(example));
    }

    public float GetQuantity(Item example)
    {
        return Manifest.GetQuantity(example);
    }

    public float GetSize(PocketType type)
    {
        return Pockets
            .Where(pocket => pocket.Type == type)
            .Sum(pocket => pocket.Size * pocket.Item.Quantity);
    }

    public float GetSpaceUsed(PocketType type)
    {
        return Manifest.Samples
            .Where(sample => sample.Type == type)
            .Sum(sample => Manifest.GetQuantity(sample) * 
                           Pocket.GetUnitSize(sample)); 
    }

    public float GetSpaceAvailable(PocketType type)
    {
        return GetSize(type) - GetSpaceUsed(type);
    }

    public float GetSpaceAvailable(Item example)
    {
        return GetSpaceAvailable(example.Type);
    }

    public bool CanFit(Item example, float quantity)
    {
        return GetSpaceAvailable(example) >= 
               example.UnitSize * quantity;
    }

    public IEnumerable<PocketType> GetPocketTypes()
    {
        return System.Enum.GetValues(typeof(PocketType)).Cast<PocketType>();
    }

    public void Touch(System.Action<Item> Action, System.Func<Item, bool> Predicate)
    {
        foreach (Item item in Items.Where(Predicate))
            Action(item);
    }
}
