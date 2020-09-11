using UnityEngine;
using System.Collections;

public class FuelBarButton : Button
{
    public FuelBar FuelBar;

    protected override void OnButtonUp()
    {
        FuelBar.State = 
            FuelBar.State == FuelBar.MeasurementState.Tonnes ? 
            FuelBar.MeasurementState.MetersPerSecond :
            FuelBar.MeasurementState.Tonnes;
    }
}
