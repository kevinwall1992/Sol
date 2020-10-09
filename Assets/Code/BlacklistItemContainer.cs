using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlacklistItemContainer : ItemContainer.Script, 
                                      ItemContainer.Filter
{
    public List<string> ProhibitedItems;

    public bool IsStorable(Item item)
    {
        return !ProhibitedItems.Contains(item.Name);
    }
}
