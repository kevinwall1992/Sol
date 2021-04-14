using UnityEngine;
using System.Collections;


[ExecuteAlways]
public class Item : MonoBehaviour
{
    public User Owner;

    public string Name, Units, ShortName, Qualifier;

    [TextArea(15, 20)]
    public string Description;

    public float Quantity;

    public Sprite ProfilePicture, Icon;

    public System.Func<string> GetQuantityString { get; set; }

    public ItemContainer Container
    { get { return GetComponentInParent<ItemContainer>(); } }

    void Start()
    {
        
    }

    void Update()
    {
        Craft craft = this.Craft();
        if (Owner == null && craft != null)
            Owner = craft.Item.Owner;

        if (GetQuantityString == null)
            GetQuantityString = () => Quantity.ToShortString() + Units;
    }

    public Item RemoveQuantity(float quantity_removed)
    {
        quantity_removed = Mathf.Min(quantity_removed, Quantity);

        Item removed = GameObject.Instantiate(this);

        Quantity = Quantity - quantity_removed;
        removed.Quantity = quantity_removed;

        return removed;
    }

    public Item Copy()
    {
        Item copy = RemoveQuantity(0);
        copy.Quantity = Quantity;

        return copy;
    }

    [RequireComponent(typeof(Item))]
    public abstract class Script : MonoBehaviour
    {
        public Item Item { get { return GetComponent<Item>(); } }
    }
}


public static class ItemExtensions
{
    public static bool IsPhysical(this Item item) { return item.Physical() != null; }
    public static PhysicalItem Physical(this Item item) { return item.GetComponent<PhysicalItem>(); }

    public static bool IsPerishable(this Item item) { return item.Perishable() != null; }
    public static PerishableItem Perishable(this Item item) { return item.GetComponent<PerishableItem>(); }

    public static bool IsLiquid(this Item item) { return item.Liquid() != null; }
    public static LiquidItem Liquid(this Item item) { return item.GetComponent<LiquidItem>(); }

    public static bool IsGas(this Item item) { return item.Gas() != null; }
    public static GasItem Gas(this Item item) { return item.GetComponent<GasItem>(); }

    public static bool IsPeople(this Item item) { return item.People(); }
    public static People People(this Item item) { return item.GetComponent<People>(); }

    public static float Mass(this Item item)
    { return item.Physical().Mass; }

    public static float SetMass(this Item item, float mass)
    { return item.Physical().Mass = mass; }

    public static Item RemoveMass(this Item item, float mass_removed)
    { return item.RemoveQuantity(mass_removed / item.Physical().MassPerUnit); }

    public static float GetMassPerUnit(this Item item)
    { return item.Physical().MassPerUnit; }

    public static float Volume(this Item item)
    { return item.Physical().Volume; }

    public static Item RemoveVolume(this Item item, float volume_removed)
    { return item.RemoveQuantity(volume_removed / item.Physical().VolumePerUnit); }

    public static float SetVolume(this Item item, float volume)
    { return item.Physical().Volume = volume; }

    public static float GetVolumePerUnit(this Item item)
    { return item.Physical().VolumePerUnit; }

    public static float Temperature(this Item item)
    { return item.Physical().Temperature; }

    public static float Moles(this Item item)
    { return item.Gas().Moles; }

    public static float Pressure(this Item item)
    { return item.Gas().Pressure; }

    public static float Condition(this Item item)
    { return item.Perishable().Condition; }

    public static bool IsSolid(this Item item)
    { return !item.IsLiquid() && !item.IsGas(); }

    public static Craft Craft(this Item item)
    { return item.GetComponentInParent<Craft>(); }

    public static Station Station(this Item item)
    { return item.GetComponentInParent<Station>(); }
}
