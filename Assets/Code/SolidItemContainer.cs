using UnityEngine;
using System.Collections;

public class SolidItemContainer : ItemContainer.Script,
                                  ItemContainer.Filter
{
    public bool IsStorable(Item item)
    {
        return item.IsPhysical() && item.IsSolid();
    }
}
