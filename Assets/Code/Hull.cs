using UnityEngine;
using System.Collections;

public class Hull : MonoBehaviour, Craft.Part
{
    public float Mass;

    public float Strength = 1000;
    public float Damage = 0;

    public float PartMass
    {
        get { return Mass; }
    }
}
