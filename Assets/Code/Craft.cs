using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ExecuteAlways]
[RequireComponent(typeof(SystemMapObject))]
public class Craft : MonoBehaviour
{
    public RectTransform PartsContainer;

    public SystemMapObject SystemMapObject
    { get { return GetComponent<SystemMapObject>(); } }

    public string Name { get { return SystemMapObject.Name; } }

    public SystemMapObject Primary
    { get { return SystemMapObject.Primary; } }

    public SatelliteMotion Motion
    {
        get { return SystemMapObject.Motion; }
        set { SystemMapObject.ChangeMotion(value); }
    }

    public IEnumerable<Part> Parts
    { get { return PartsContainer.GetComponentsInChildren<Part>(); } }

    public Hull Hull { get { return GetPart<Hull>(); } }

    public Engine Engine { get { return GetPart<Engine>(); } }
    public bool HasEngine { get { return Engine != null; } }

    public Navigation Navigation { get { return GetPart<Navigation>(); } }
    public bool HasNavigation { get { return Navigation != null; } }

    public IEnumerable<ItemContainer> Storage
    {
        get
        {
            return GetPartsOfType<ItemContainer>()
                .Where(container => !container.HasComponent<LifeSupport>())
                .Where(container => !HasEngine ? true : !Engine.Tanks.Values
                    .Select(tank => tank.Container).Contains(container));
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

    void Update()
    {
        SystemMapObject.Mass = Mass;

        if(Primary != null && HasEngine)
            SystemMapObject.ImageCanvasGroup.transform.rotation =
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
