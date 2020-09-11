using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface Visitable
{
    string PlaceName { get; }
    string PlaceDescription { get; }

    IEnumerable<Craft> Visitors { get; }

    bool IsWelcome(Craft craft);
    SatelliteMotion GetVisitingMotion(Craft craft);
}
