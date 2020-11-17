using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SolidItemContainer))]
public class Hold : ItemContainer.Script, ItemContainer.Filter
{
    public bool IsStorable(Item item)
    {
        return !item.IsPeople();
    }
}
