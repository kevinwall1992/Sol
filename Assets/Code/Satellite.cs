using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;

[ExecuteAlways]
public class Satellite : MonoBehaviour
{
    public SatelliteMotion Motion;

    public RectTransform SatellitesContainer;

    public string Name;
    public float Mass;
    public float Radius;

    public Vector3 Position { get { return Motion.Position; } }

    public Satellite Primary
    { get { return Motion.Primary; } }

    public IEnumerable<Satellite> Satellites
    {
        get
        {
            return SatellitesContainer.GetComponentsInChildren<Satellite>()
                .Where(satellite => satellite.Primary == this);
        }
    }

    protected virtual void Update()
    {
        if (!Application.isPlaying)
        {
            Motion.Primary = transform.parent.GetComponentInParent<Satellite>();

            int sibling_index = transform.GetSiblingIndex();
            if (sibling_index > 0)
            {
                Satellite sibling =
                    transform.parent.GetChild(sibling_index - 1)
                    .GetComponent<Satellite>();

                if (sibling.Motion.Periapsis > Motion.Periapsis)
                    transform.SetAsFirstSibling();
            }
        }
        
        if (Motion.Periapsis > Motion.Apoapsis)
            Utility.Swap(ref Motion.Periapsis, ref Motion.Apoapsis);

        if (gameObject.name != Name)
            gameObject.name = Name;
    }

    public void ChangeMotion(SatelliteMotion motion)
    {
        Motion = motion;

        if (Primary != null &&
            transform.parent != Motion.Primary.SatellitesContainer)
            transform.SetParent(Primary.SatellitesContainer);
    }
}


public static class SatelliteExtensions
{
    public static Craft Craft(this Satellite satellite)
    {
        return satellite.GetComponent<Craft>();
    }

    public static bool IsCraft(this Satellite satellite)
    {
        return satellite.Craft() != null;
    }

    public static Station Station(this Satellite satellite)
    {
        return satellite.GetComponent<Station>();
    }

    public static bool IsStation(this Satellite satellite)
    {
        return satellite.Station() != null;
    }

    public static NaturalSatellite NaturalSatellite(this Satellite satellite)
    {
        return satellite.GetComponent<NaturalSatellite>();
    }

    public static bool IsNaturalSatellite(this Satellite satellite)
    {
        return satellite.NaturalSatellite() != null;
    }

    public static Visitable Place(this Satellite satellite)
    {
        if (satellite.IsStation())
            return satellite.Station();
        else if (satellite.IsNaturalSatellite())
            return satellite.NaturalSatellite();

        return null;
    }

    public static bool IsVisitable(this Satellite satellite)
    {
        return satellite.Place() != null;
    }

    public static SystemMapObject SystemMapObject(this Satellite satellite)
    {
        return satellite.GetComponent<SystemMapObject>();
    }
}
