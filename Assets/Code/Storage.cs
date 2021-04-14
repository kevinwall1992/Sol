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
    { get { return ItemContainers.SelectMany(container => container.Items.Values); } }

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

    public float GetVolumeOf(string item_name)
    {
        return ItemContainers.Sum(
            container => container.Contains(item_name) ? 
                         container.GetItem(item_name).Volume() : 
                         0);
    }

    public float GetUnusedVolumeFor(Item item)
    {
        float available_volume = 0;

        foreach(ItemContainer item_container in ItemContainers)
            if (item_container.IsStorable(item))
                available_volume += item_container.AvailableVolume;

        return available_volume;
    }

    //Maximum volume without removing other items that are 
    //competing for space
    public float GetMaximumVolumeOf(Item item)
    {
        return GetVolumeOf(item.Name) + GetUnusedVolumeFor(item);
    }

    //Maximum volume if allowed to remove competing items
    public float GetIdealMaximumVolumeOf(Item item)
    {
        float ideal_volume = 0;

        foreach (ItemContainer item_container in ItemContainers)
            if (item_container.IsStorable(item))
                ideal_volume += item_container.Volume;

        return ideal_volume;
    }

    public bool CanFit(Item item)
    {
        return GetUnusedVolumeFor(item) >= item.Volume();
    }

    public bool CanFit(Item item, float quantity)
    {
        return GetUnusedVolumeFor(item) >= 
               item.Physical().VolumePerUnit * quantity;
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

    public float StoreQuantity(Item sample, float quantity)
    {
        foreach (ItemContainer item_container in ItemContainers)
        {
            quantity = item_container.PutInQuantity(sample, quantity);

            if (quantity == 0)
                break;
        }

        return quantity;
    }

    public float GetQuantity(string name)
    {
        float quantity = 0;

        foreach (ItemContainer item_container in ItemContainers)
            quantity += item_container.GetQuantity(name);

        return quantity;
    }

    public bool Contains(string name)
    {
        return GetQuantity(name) > 0;
    }

    public Item GetSampleItem(string name)
    {
        if (GetQuantity(name) == 0)
            return null;

        return ItemContainers
            .FirstOrDefault(container => container.Contains(name))
            .GetItem(name);
    }

    public IEnumerable<Item> GetSampleItems()
    {
        return Items.Distinct(item => item.Name);
    }

    public Item Retrieve(string name, float quantity = -1)
    {
        float quantity_available = GetQuantity(name);

        if (quantity < 0 || quantity > quantity_available)
            quantity = quantity_available;

        Item retrieved_item = null;

        foreach (ItemContainer item_container in ItemContainers)
        {
            if(item_container.Contains(name))
            {
                if (retrieved_item == null)
                    retrieved_item = item_container.TakeOut(name, quantity);
                else
                    retrieved_item.Quantity += 
                        item_container.TakeOutQuantity(name, quantity);

                if (retrieved_item.Quantity >= quantity)
                    break;
            }
        }

        return retrieved_item;
    }

    public float RetrieveQuantity(string name, float quantity)
    {
        if (quantity == 0)
            return 0;

        Item retrieved_item = Retrieve(name, quantity);
        quantity = retrieved_item.Quantity;

        GameObject.Destroy(retrieved_item.gameObject);
        return quantity;
    }

    public float SendTo(Storage destination, string name, float quantity)
    {
        quantity = Mathf.Min(quantity, GetQuantity(name));

        destination.StoreQuantity(GetSampleItem(name), quantity);

        return quantity;
    }

    public void TouchItems(System.Action<Item> Touch, System.Func<Item, bool> Predicate = null)
    {
        foreach(ItemContainer container in ItemContainers)
        {
            foreach (Item item in container.Items.Values)
                if (Predicate == null || Predicate(item))
                    Touch(item);
        }
    }

    public Storage GetSubset(System.Func<ItemContainer, bool> predicate)
    {
        return new Storage(ItemContainers.Where(predicate));
    }
}
