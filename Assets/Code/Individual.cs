using UnityEngine;
using System.Collections;

[RequireComponent(typeof(People))]
public class Individual : Item.Script
{
    private void Update()
    {
        Item.Quantity = 1;
    }
}
