using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Mine : MonoBehaviour, Craft.Part
{
    public float PartMass
    {
        get
        {
            return MineralDeposits.Sum(
                deposit => deposit.Container.PartMass);
        }
    }

    public IEnumerable<MineralDeposit> MineralDeposits
    { get { return GetComponentsInChildren<MineralDeposit>(); } }
}
