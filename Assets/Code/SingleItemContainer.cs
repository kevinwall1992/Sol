using UnityEngine;
using System.Collections;

public class SingleItemContainer : ItemContainer.Script, 
                                   ItemContainer.Filter
{
    public Item AllowedItem;

    public bool IsStorable(Item item)
    {
        return item.Name == AllowedItem.Name;
    }
}
