using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Craft))]
public class Station : MonoBehaviour, Visitable
{
    public Market OfficialMarket;

    public string PlaceName { get { return Craft.Satellite.Name; } }

    [SerializeField]
    string description = "";
    public string PlaceDescription { get { return description; } }

    public Craft Craft { get { return GetComponent<Craft>(); } }

    public IEnumerable<Craft> Visitors
    {
        get
        {
            return transform.Children()
                .Where(child => child.HasComponent<Craft>())
                .Select(child => child.GetComponent<Craft>());
        }
    }

    public bool IsWelcome(Craft craft)
    {
        return true;
    }

    public SatelliteMotion GetVisitingMotion(Craft craft)
    {
        float velocity = 0.0001f;
        float distance = MathConstants.GravitationalConstant * Craft.Mass / 
                         Mathf.Pow(velocity, 2);

        return new SatelliteMotion(
            Craft.Satellite, 
            distance, distance, 
            0, Random.value * 2 * Mathf.PI);
    }
}
