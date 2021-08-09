using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Pocket))]
public class Bay : Craft.Part
{
    public Pocket Pocket { get { return GetComponent<Pocket>(); } }
}
