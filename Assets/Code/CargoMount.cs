using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CargoMount : MonoBehaviour, Craft.Part
{
    public float Mass;

    public float PartMass { get { return Mass; } }

    public IEnumerable<CargoContainer> CargoContainers
    { get { return transform.Children().SelectComponents<CargoContainer>(); } }
}
