using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Storage
{
    public System.Func<IEnumerable<ItemContainer>> GetItemContainers;
    public IEnumerable<ItemContainer> ItemContainers
    { get { return GetItemContainers(); } }

    public float DryMass { get { return ItemContainers.Sum(container => container.PartMass); } }
    public float ItemMass { get { return ItemContainers.Sum(container => container.ItemMass); } }
    public float TotalMass { get { return DryMass + ItemMass; } }

    public IEnumerable<Item> Items
    { get { return ItemContainers.SelectMany(container => container.Items); } }

    public Manifest Manifest { get { return Items.ToManifest(); } }

    public Storage(System.Func<IEnumerable<ItemContainer>> GetItemContainers = null)
    {
        this.GetItemContainers = GetItemContainers;
    }

    public Storage(IEnumerable<ItemContainer> item_containers)
    {
        this.GetItemContainers = () => item_containers;
    }

    public Storage(IEnumerable<ItemContainer.Script> item_containers)
    {
        this.GetItemContainers = 
            () => item_containers.Select(container => container.Container);
    }

    public float GetVolumeOf(Item example)
    {
        if (example == null)
            return 0;

        return ItemContainers.Sum(
            container => container.Contains(example) ? 
                         container.GetItem(example).Volume() : 
                         0);
    }

    public float GetUnusedVolumeFor(Item example)
    {
        float available_volume = 0;

        foreach(ItemContainer item_container in ItemContainers)
            if (item_container.IsStorable(example))
                available_volume += item_container.AvailableVolume;

        return available_volume;
    }

    //Maximum volume without removing other items that are 
    //competing for space
    public float GetMaximumVolumeOf(Item example)
    {
        return GetVolumeOf(example) + GetUnusedVolumeFor(example);
    }

    //Maximum volume if allowed to remove competing items
    public float GetIdealMaximumVolumeOf(Item example)
    {
        float ideal_volume = 0;

        foreach (ItemContainer item_container in ItemContainers)
            if (item_container.IsStorable(example))
                ideal_volume += item_container.Volume;

        return ideal_volume;
    }

    public bool CanFit(Item example)
    {
        return GetUnusedVolumeFor(example) >= example.Volume();
    }

    public bool CanFit(Item example, float quantity)
    {
        return GetUnusedVolumeFor(example) >= 
               example.Physical().VolumePerUnit * quantity;
    }

    public bool Store(Item item)
    {
        foreach(ItemContainer item_container in ItemContainers)
        {
            if (item_container.IsStorable(item))
            {
                bool was_sucessful = item_container.PutIn(item);

                if (was_sucessful)
                    return true;
            }
        }

        return false;
    }

    public float StoreQuantity(Item example, float quantity)
    {
        foreach (ItemContainer item_container in ItemContainers)
        {
            quantity = item_container.PutInQuantity(example, quantity);

            if (quantity == 0)
                break;
        }

        return quantity;
    }

    public float GetQuantity(Item example)
    {
        float quantity = 0;

        foreach (ItemContainer item_container in ItemContainers)
            quantity += item_container.GetQuantity(example);

        return quantity;
    }

    public bool Contains(Item example)
    {
        return GetQuantity(example) > 0;
    }

    public Item Retrieve(Item example, float quantity = -1)
    {
        float quantity_available = GetQuantity(example);

        if (quantity < 0 || quantity > quantity_available)
            quantity = quantity_available;

        Item retrieved_item = null;

        foreach (ItemContainer item_container in ItemContainers)
        {
            if(item_container.Contains(example))
            {
                if (retrieved_item == null)
                    retrieved_item = item_container.TakeOut(example, quantity);
                else
                    retrieved_item.Quantity += 
                        item_container.TakeOutQuantity(example, quantity);

                if (retrieved_item.Quantity >= quantity)
                    break;
            }
        }

        return retrieved_item;
    }

    public float RetrieveQuantity(Item example, float quantity)
    {
        if (quantity == 0)
            return 0;

        Item retrieved_item = Retrieve(example, quantity);
        quantity = retrieved_item.Quantity;

        GameObject.Destroy(retrieved_item.gameObject);
        return quantity;
    }

    public float SendTo(Item example, float quantity, Storage destination)
    {
        quantity = Mathf.Min(quantity, GetQuantity(example));

        destination.StoreQuantity(example, quantity);

        return quantity;
    }

    public void TouchItems(System.Action<Item> Touch, System.Func<Item, bool> Predicate = null)
    {
        foreach(ItemContainer container in ItemContainers)
        {
            foreach (Item item in container.Items)
                if (Predicate == null || Predicate(item))
                    Touch(item);
        }
    }

    public Storage GetSubset(System.Func<ItemContainer, bool> predicate)
    {
        return new Storage(ItemContainers.Where(predicate));
    }
}
