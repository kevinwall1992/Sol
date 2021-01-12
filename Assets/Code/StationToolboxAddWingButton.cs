using UnityEngine;
using System.Collections;
using System.Linq;

public class StationToolboxAddWingButton : StationToolboxButton.Interactor
{
    public Room RoomPrefab;

    public Material Material;

    private void Start()
    {
        Overlay.LineRenderer.material = Material;
    }

    public override bool OnInteract()
    {
        Ring.Floor floor = Ring.GetFloor(Radius);
        if (floor == null)
            return false;

        Ring.Floor.Wing wing = floor.GetWing(Radians);
        if (wing != null)
            return false;

        float position = 
            (floor.RadiansToMeters(Radians) / Ring.UnitWingWidth).RoundDown() * 
            Ring.UnitWingWidth;
        float radian_position = floor.MetersToRadians(position);

        if (InputUtility.WasMouseLeftReleased)
            floor.Wings.Add(new Ring.Floor.Wing(floor, position, 1));
        else
        {
            Overlay.StartRadians = radian_position;
            Overlay.EndRadians = radian_position + 
                                 floor.MetersToRadians(Ring.UnitWingWidth);

            Overlay.StartRadius = floor.Radius - 
                                  floor.CeilingHeight;
            Overlay.EndRadius = floor.Radius;
        }

        return true;
    }
}
