using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Mine : Craft.Part
{
    public IEnumerable<MineralDeposit> MineralDeposits
    { get { return GetComponentsInChildren<MineralDeposit>(); } }
}
