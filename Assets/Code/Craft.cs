using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ExecuteAlways]
[RequireComponent(typeof(Satellite))]
public class Craft : MonoBehaviour
{
    public User Owner;

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

    public CargoMount CargoMount { get { return GetPart<CargoMount>(); } }
    public bool HasCargoMount { get { return CargoMount != null; } }

    public Engine Engine { get { return GetPart<Engine>(); } }
    public bool HasEngine { get { return Engine != null; } }

    public Navigation Navigation { get { return GetPart<Navigation>(); } }
    public bool HasNavigation { get { return Navigation != null; } }

    public Storage Cargo
    {
        get
        {
            if (HasCargoMount)
                return new Storage(CargoMount.CargoContainers
                    .Select(cargo_container => cargo_container.Container));
            else
                return new Storage();
        }
    }

    public IEnumerable<LifeSupport> LifeSupport
    {
        get
        {
            return GetPartsOfType<ItemContainer>()
                .Where(container => container.HasComponent<LifeSupport>())
                .Select(container => container.GetComponent<LifeSupport>());
        }
    }

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
            return DryMass +
                   GetComponentsInChildren<Item>().Sum(item => item.Mass());
        }
    }

    public float EmptyTankMass
    { get { return Mass - (HasEngine ? Engine.PropellentMass : 0); } }

    public float CurbMass { get { return Mass - Cargo.ItemMass; } }

    public Station Station
    { get { return transform.parent.GetComponentInParent<Station>(); } }

    public bool IsDocked { get { return Station != null; } }

    void Update()
    {
        Satellite.Mass = Mass;

        if(Primary != null && HasEngine)
            this.Satellite.SystemMapObject().ImageCanvasGroup.transform.rotation =
                Quaternion.Euler(0, 0, MathUtility.RadiansToDegrees(
                    Motion.TrueAnomaly + Motion.ArgumentOfPeriapsis));
    }

    public IEnumerable<T> GetPartsOfType<T>() where T : class, Part
    {
        return Parts.Where(part => part is T).Select(part => part as T);
    }

    public T GetPart<T>() where T : class, Part
    {
        IEnumerable<T> parts = GetPartsOfType<T>();
        if (parts.Count() == 0)
            return null;

        return parts.First();
    }


    public interface Part
    {
        float PartMass { get; }
    }
}


public static class CraftPartExtensions
{
    public static Craft Craft(this Craft.Part part)
    { return (part as MonoBehaviour).GetComponentInParent<Craft>(); }
}
