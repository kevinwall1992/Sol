using UnityEngine;
using System.Collections;
using System;

public class ComparableInventoryElement : ComparableElement
{
    public override IComparable Comparable
    {
        get
        {
            Item item = InventoryPage.GetItemFromInventoryElement(gameObject);

            if (item == null)
                return 0;

            return item.ShortName;
        }
    }
}
