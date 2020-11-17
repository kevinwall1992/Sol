using UnityEngine;
using System.Collections;

public class LifeSupport : ItemContainer.Script, ItemContainer.Filter
{
    public bool IsStorable(Item item)
    {
        return item.IsPeople();
    }
}
