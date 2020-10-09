using UnityEngine;
using System.Collections;

[ExecuteAlways]
[RequireComponent(typeof(PhysicalItem))]
public class GasItem : Item.Script
{
    public float KilogramsPerMole;

    public float Moles { get { return Item.Mass() / KilogramsPerMole; } }

    public float Pressure
    {
        get
        {
            return Moles *
                   MathConstants.IdealGasConstant * 
                   Item.Temperature() / 
                   Item.Volume();
        }

        set
        {
            Item.Physical().VolumePerUnit =
                Moles *
                MathConstants.IdealGasConstant *
                Item.Temperature() /
                value;
        }
    }
}
