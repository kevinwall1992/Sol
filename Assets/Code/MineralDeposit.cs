using UnityEngine;
using System.Collections;

public class MineralDeposit : ItemContainer.Script, 
                              ItemContainer.Filter
{
    public bool IsStorable(Item item)
    {
        return item.HasComponent<Mineral>();
    }
}
