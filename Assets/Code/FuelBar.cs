using UnityEngine;
using System.Collections;

public class FuelBar : ProgressBar
{
    public enum MeasurementState { Tonnes, MetersPerSecond }
    public MeasurementState State;

    protected override void Update()
    {
        if (State == MeasurementState.Tonnes)
            Units = "t";
        else
            Units = "km/s";

        base.Update();
    }
}
