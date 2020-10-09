using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ExecuteAlways]
[RequireComponent(typeof(SingleItemContainer))]
public class FluidContainer : ItemContainer.Script, 
                              ItemContainer.Filter
{
    SingleItemContainer SingleItemContainer
    { get { return GetComponent<SingleItemContainer>(); } }

    public Item AllowedFluid
    { get { return SingleItemContainer.AllowedItem; } }

    public bool IsStorable(Item item)
    {
        if (item.IsSolid())
            return false;

        if (AllowedFluid != null)
            return AllowedFluid.Name == item.Name;

        return true;
    }

    private void Update()
    {
        if (SingleItemContainer.AllowedItem != null &&
            !IsStorable(SingleItemContainer.AllowedItem))
            SingleItemContainer.AllowedItem = null;
    }
}
