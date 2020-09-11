using UnityEngine;
using System.Collections;

public class PhysicalItem : Item.Script
{
    public float MassPerUnit;
    public float VolumePerUnit;

    public float SpecificHeatCapacity;

    public float DefaultTemperature = 294;
    public float Temperature
    {
        get { return HeatEnergy / SpecificHeatCapacity; }
        set { HeatEnergy = SpecificHeatCapacity * value * Mass; }
    }

    [ReadOnly]
    public float HeatEnergy;

    public float Mass
    {
        get { return MassPerUnit * Item.Quantity; }
        set { Item.Quantity = value / MassPerUnit; }
    }
    public float Volume
    {
        get { return VolumePerUnit * Item.Quantity; }
        set { Item.Quantity = value / VolumePerUnit; }
    }

    private void Start()
    {
        Temperature = DefaultTemperature;
    }
}
