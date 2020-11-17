using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Room))]
[RequireComponent(typeof(LifeSupport))]
public class Housing : ItemContainer.Script
{
    public float CeilingHeight = 2.4f;

    public float SquareFootage
    { get { return Container.Item.Volume() / CeilingHeight; } }
}
