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

    public IEnumerable<Module> Modules
    { get { return GetComponentsInChildren<Module>(); } }
    public IEnumerable<Ring> Rings
    { get { return Modules.SelectComponents<Module, Ring>(); } }

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

    public IEnumerable<Room> GetRooms(User owner)
    {
        return Craft.GetInventory(owner).Items
            .SelectComponents<Item, Room>();
    }

    public class Module : Craft.Part
    {

    }
}
