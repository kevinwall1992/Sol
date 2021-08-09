using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ExecuteAlways]
[RequireComponent(typeof(Satellite))]
[RequireComponent(typeof(Item))]
[RequireComponent(typeof(Owned))]
public class Craft : Item.Script
{
    public RectTransform PartsContainer;

    public Satellite Satellite
    { get { return GetComponent<Satellite>(); } }

    public string Name { get { return Satellite.Name; } }

    public Satellite Primary
    { get { return Satellite.Primary; } }

    public SatelliteMotion Motion
    {
        get { return Satellite.Motion; }
        set { Satellite.ChangeMotion(value); }
    }

    public IEnumerable<Part> Parts
    { get { return PartsContainer.GetComponentsInChildren<Part>(); } }

    public Hull Hull { get { return GetPart<Hull>(); } }

    public Engine Engine { get { return GetPart<Engine>(); } }
    public bool HasEngine { get { return Engine != null; } }

    public Navigation Navigation { get { return GetPart<Navigation>(); } }
    public bool HasNavigation { get { return Navigation != null; } }

    public float DryMass
    {
        get
        {
            return Parts.Sum(part => part.PartMass);
        }
    }

    public float Mass
    {
        get
        {
            return GetComponentsInChildren<Item>()
                .Where(item => item.IsPhysical())
                .Sum(item => item.Mass());
        }
    }

    public float EmptyTankMass
    { get { return Mass - (HasEngine ? Engine.PropellentMass : 0); } }

    public float CurbMass
    { get { return Mass - CargoMass; } }

    public Station Station
    { get { return Item.Station(); } }

    public bool IsDocked { get { return Station != null; } }

    public RectTransform InventoryContainer;

    public IEnumerable<Inventory> Inventories
    { get { return InventoryContainer.GetComponentsInChildren<Inventory>(); } }

    public Inventory Cargo
    { get { return GetInventory(this.GetOwner()); } }

    public float CargoMass
    {
        get
        {
            return Cargo.Items
                .Where(item => item.IsPhysical())
                .Sum(item => item.Mass());
        }
    }

    void Update()
    {
        Satellite.Mass = Mass;

        if(Primary != null && HasEngine)
            this.Satellite.SystemMapObject().ImageCanvasGroup.transform.rotation =
                Quaternion.Euler(0, 0, MathUtility.RadiansToDegrees(
                    Motion.TrueAnomaly + Motion.ArgumentOfPeriapsis));

        if(Application.isPlaying)
            foreach (Part part in Parts)
                if (!part.IsInstalled)
                    part.Install();
    }

    public IEnumerable<T> GetPartsOfType<T>() where T : Part
    {
        return Parts.Where(part => part is T).Select(part => part as T);
    }

    public T GetPart<T>() where T : Part
    {
        IEnumerable<T> parts = GetPartsOfType<T>();
        if (parts.Count() == 0)
            return null;

        return parts.First();
    }

    public Inventory GetInventory(User owner)
    {
        IEnumerable<Inventory> matches = 
            Inventories.Where(inventory_ => 
                inventory_.HasComponent<Owned>() && 
                inventory_.GetComponent<Owned>().Owner == owner);

        Inventory inventory;
        if (matches.Count() == 0)
        {
            inventory = new GameObject("Inventory").AddComponent<Inventory>();
            inventory.transform.SetParent(InventoryContainer);

            Owned owned = inventory.gameObject.AddComponent<Owned>();
            owned.Owner = owner;
        }
        else
        {
            inventory = matches.First();

            if (matches.Count() > 1)
            {
                foreach (Inventory other in matches)
                {
                    if (other == inventory)
                        continue;

                    inventory.PutIn(other);

                    Destroy(other.gameObject);
                }
            }
        }

        return inventory;
    }


    [RequireComponent(typeof(PhysicalItem))]
    public class Part : Item.Script
    {
        bool is_installed = false;
        public bool IsInstalled { get { return is_installed; } }

        public float PartMass { get { return Item.Mass(); } }

        public virtual Manifest BoundItems
        { get { return new Manifest(); } }

        public virtual bool Install()
        {
            Inventory cargo = Item.Craft().Cargo;

            foreach(Item sample in BoundItems.Samples)
                cargo.PutIn(sample, BoundItems[sample]);

            is_installed = true;
            return true;
        }

        public virtual bool Uninstall()
        {
            Inventory cargo = Item.Craft().Cargo;

            foreach (Item sample in BoundItems.Samples)
                if (cargo.GetQuantity(sample) < BoundItems[sample])
                    return false;

            foreach (Item sample in BoundItems.Samples)
                cargo.TakeOut(sample, BoundItems[sample]);

            is_installed = false;
            return true;
        }
    }
}


public static class CraftPartExtensions
{
    public static Craft Craft(this Craft.Part part)
    { return part.Item.Craft(); }
}
