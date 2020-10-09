using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StationManagement : User.Script
{   
    public IEnumerable<Station> Stations
    { get { return User.Crafts.SelectComponents<Craft, Station>(); } }
}
