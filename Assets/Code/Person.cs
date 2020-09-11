using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhysicalItem))]
[RequireComponent(typeof(PerishableItem))]
public class Person : Item.Script
{
    private void Update()
    {
        Item.Quantity = 1;
    }
}
