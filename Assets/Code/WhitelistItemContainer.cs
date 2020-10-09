using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WhitelistItemContainer : ItemContainer.Script, 
                                      ItemContainer.Filter
{
    public List<string> AllowedItems;

    public bool IsStorable(Item item)
    {
        return AllowedItems.Contains(item.Name);
    }
}
