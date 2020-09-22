using UnityEngine;
using System.Collections;

public class RefuelAmountButton : Button
{
    public float RequiredFuelMass;

    public RefuelBar RefuelBar
    { get { return GetComponentInParent<RefuelBar>(); } }

    protected override void OnButtonUp()
    {
        RefuelBar.TargetFuelMass = RequiredFuelMass;
    }
}
