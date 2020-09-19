using UnityEngine;
using System.Collections;
using System;

public class ComparableInventoryElement : ComparableElement
{
    Item Item
    {
        get
        {
            if (this.HasComponent<IconInventoryElement>())
                return GetComponent<IconInventoryElement>().Item;
            else if(this.HasComponent<LineInventoryElement>())
                return GetComponent<LineInventoryElement>().Item;
            else
                return null;
        }
    }

    public override IComparable Comparable
    {
        get
        {
            if (Item == null)
                return 0;

            return Item.ShortName;
        }
    }
}
