using UnityEngine;
using System.Collections;

[RequireComponent(typeof(FluidContainer))]
public class GasContainer : ItemContainer.Script, 
                            ItemContainer.Filter, 
                            ItemContainer.Packer
{
    public float Pressure;

    public bool IsStorable(Item item)
    {
        return !item.IsSolid();
    }

    public void Pack(Item item)
    {
        item.GasItem().Pressure = Pressure;
    }

    public void Unpack(Item item)
    {
        item.GasItem().Pressure = MathConstants.StandardPressure;
    }

    private void Update()
    {

    }
}
