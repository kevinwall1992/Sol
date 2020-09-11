using UnityEngine;
using System.Collections;

[RequireComponent(typeof(FluidContainer))]
public class LiquidContainer : ItemContainer.Script, 
                               ItemContainer.Filter
{
    public bool IsStorable(Item item)
    {
        return item.IsLiquid();
    }
}
