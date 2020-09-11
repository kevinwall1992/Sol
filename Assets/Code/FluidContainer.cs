using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class FluidContainer : ItemContainer.Script, 
                              ItemContainer.Filter
{
    public Item Fluid
    {
        get
        {
            if (Container.Items.Count() == 0)
                return null;

            return Container.Items.Values.First();
        }
    }

    public bool IsStorable(Item item)
    {
        if (item.IsSolid())
            return false;

        if (Fluid != null)
            return Fluid.Name == item.Name;

        return true;
    }
}
