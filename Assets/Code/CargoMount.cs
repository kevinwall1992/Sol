using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CargoMount : Craft.Part
{
    public IEnumerable<CargoContainer> CargoContainers
    { get { return transform.Children().SelectComponents<CargoContainer>(); } }
}
