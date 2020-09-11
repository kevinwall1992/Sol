using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SystemMapObject))]
public class NaturalBody : MonoBehaviour, Visitable
{
    public string PlaceName { get { return SystemMapObject.Name; } }

    [SerializeField]
    string description = "";
    public string PlaceDescription { get { return description; } }

    public IEnumerable<Craft> Visitors
    { get { return new List<Craft>(); } }

    public SystemMapObject SystemMapObject
    { get { return GetComponent<SystemMapObject>(); } }

    public SatelliteMotion GetVisitingMotion(Craft craft)
    {
        return null;
    }

    public bool IsWelcome(Craft craft)
    {
        return false;
    }
}
