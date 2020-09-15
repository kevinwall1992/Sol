using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Satellite))]
public class NaturalSatellite : MonoBehaviour, Visitable
{
    public string PlaceName { get { return Satellite.Name; } }

    [SerializeField]
    string description = "";
    public string PlaceDescription { get { return description; } }

    public IEnumerable<Craft> Visitors
    { get { return new List<Craft>(); } }

    public Satellite Satellite
    { get { return GetComponent<Satellite>(); } }

    public SatelliteMotion GetVisitingMotion(Craft craft)
    {
        return null;
    }

    public bool IsWelcome(Craft craft)
    {
        return false;
    }
}
