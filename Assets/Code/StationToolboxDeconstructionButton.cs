using UnityEngine;
using System.Collections;
using System.Linq;

public class StationToolboxDeconstructionButton : StationToolboxButton.Interactor
{
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
        {
            if (InputUtility.WasMouseLeftReleased)
                floor.Wings.Remove(wing);
            else
            {
                Overlay.StartRadians = wing.StartRadians;
                Overlay.EndRadians = wing.EndRadians;
                Overlay.StartRadius = floor.Radius - floor.CeilingHeight;
                Overlay.EndRadius = floor.Radius;
            }
        }
        else
        {
            int floor_index = Ring.Floors.IndexOf(floor);
            bool is_in_bottom_half = floor_index < Ring.Floors.Count / 2;

            if (InputUtility.WasMouseLeftReleased)
            {
                if (is_in_bottom_half)
                    Ring.Floors.RemoveRange(0, floor_index + 1);
                else
                    Ring.Floors.RemoveRange(floor_index, Ring.Floors.Count - floor_index);
            }
            else
            {
                Overlay.StartRadians = 0;
                Overlay.EndRadians = 2 * Mathf.PI;

                if (is_in_bottom_half)
                {
                    Overlay.StartRadius = floor.Radius - floor.CeilingHeight;
                    Overlay.EndRadius = Ring.GroundFloorRadius;
                }
                else
                {
                    Overlay.StartRadius = Ring.RoofRadius;
                    Overlay.EndRadius = floor.Radius;
                }
            }
        }

        return true;
    }
}
