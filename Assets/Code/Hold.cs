using UnityEngine;
using System.Collections;

public class Hold : ItemContainer.Script, ItemContainer.Filter
{
    public bool IsStorable(Item item)
    {
        return item.IsSolid() && !item.IsPerson();
    }
}
