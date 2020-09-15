using UnityEngine;
using System.Collections;


public class Item : MonoBehaviour
{
    public string Name;

    public float Quantity;

    public ItemContainer Container
    { get { return GetComponentInParent<ItemContainer>(); } }

    void Start()
    {

    }

    void Update()
    {

    }

    public Item RemoveQuantity(float quantity_removed)
    {
        if (quantity_removed >= Quantity)
            return this;

        Item removed = GameObject.Instantiate(this);

        Quantity = Quantity - quantity_removed;
        removed.Quantity = quantity_removed;

        return removed;
    }

    [RequireComponent(typeof(Item))]
    public abstract class Script : MonoBehaviour
    {
        public Item Item { get { return GetComponent<Item>(); } }
    }
}


public static class ItemExtensions
{
    public static bool IsPhysicalItem(this Item item) { return item.GetComponent<PhysicalItem>() != null; }
    public static PhysicalItem PhysicalItem(this Item item) { return item.GetComponent<PhysicalItem>(); }

    public static bool IsPerishableItem(this Item item) { return item.GetComponent<PerishableItem>() != null; }
    public static PerishableItem PerishableItem(this Item item) { return item.GetComponent<PerishableItem>(); }

    public static bool IsLiquidItem(this Item item) { return item.GetComponent<LiquidItem>() != null; }
    public static LiquidItem LiquidItem(this Item item) { return item.GetComponent<LiquidItem>(); }

    public static bool IsGasItem(this Item item) { return item.GetComponent<GasItem>() != null; }
    public static GasItem GasItem(this Item item) { return item.GetComponent<GasItem>(); }

    public static bool IsPerson(this Item item) { return item.GetComponent<Person>() != null; }
    public static Person Person(this Item item) { return item.GetComponent<Person>(); }

    public static float Mass(this Item item)
    { return item.PhysicalItem().Mass; }

    public static float SetMass(this Item item, float mass)
    { return item.PhysicalItem().Mass = mass; }

    public static Item RemoveMass(this Item item, float mass_removed)
    { return item.RemoveQuantity(mass_removed / item.PhysicalItem().MassPerUnit); }

    public static float Volume(this Item item)
    { return item.PhysicalItem().Volume; }

    public static Item RemoveVolume(this Item item, float volume_removed)
    { return item.RemoveQuantity(volume_removed / item.PhysicalItem().VolumePerUnit); }

    public static float SetVolume(this Item item, float volume)
    { return item.PhysicalItem().Volume = volume; }

    public static float Temperature(this Item item)
    { return item.PhysicalItem().Temperature; }

    public static float Moles(this Item item)
    { return item.GasItem().Moles; }

    public static float Pressure(this Item item)
    { return item.GasItem().Pressure; }

    public static float Condition(this Item item)
    { return item.PerishableItem().Condition; }

    public static bool IsSolid(this Item item)
    { return !item.IsLiquid() && !item.IsGas(); }

    public static bool IsLiquid(this Item item)
    { return item.IsLiquid(); }

    public static bool IsGas(this Item item)
    { return item.IsGas(); }
}
