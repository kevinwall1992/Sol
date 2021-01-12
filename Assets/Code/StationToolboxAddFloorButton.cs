using UnityEngine;
using System.Collections;
using System.Linq;

public class StationToolboxAddFloorButton : StationToolboxButton.Interactor
{
    public float CeilingHeight;
    public Material Material;

    public int CeilingSize
    { get { return (CeilingHeight / Ring.Floor.UnitCeilingHeight).Round(); } }

    private void Start()
    {
        Overlay.LineRenderer.material = Material;
    }

    public void Update()
    {
        CeilingHeight = 
            CeilingSize * 
            Ring.Floor.UnitCeilingHeight;
    }

    public override bool OnInteract()
    {
        bool is_basement;
        if (Radius > Ring.GroundFloorRadius &&
            Radius < Ring.GroundFloorRadius + CeilingHeight)
            is_basement = true;
        else if (Radius < Ring.RoofRadius &&
            Radius > Ring.RoofRadius - CeilingHeight)
            is_basement = false;
        else
            return false;

        if (InputUtility.WasMouseLeftReleased)
        {
            int floor_index;
            float floor_radius;
            if(is_basement)
            {
                floor_index = 0;
                floor_radius = Ring.Floors.First().Radius + 
                               CeilingHeight;
            }
            else
            {
                floor_index = Ring.Floors.Count;
                floor_radius = Ring.Floors.Last().Radius - 
                               Ring.Floors.Last().CeilingHeight;
            }

            RingVisualization.Ring.Floors.Insert(
                floor_index,
                new Ring.Floor(Ring, floor_radius, CeilingSize));

            return false;
        }

        Overlay.StartRadians = 0;
        Overlay.EndRadians = 2 * Mathf.PI;

        if (is_basement)
        {
            Overlay.StartRadius = Ring.GroundFloorRadius;
            Overlay.EndRadius = Ring.GroundFloorRadius + CeilingHeight;
        }
        else
        {
            Overlay.StartRadius = Ring.RoofRadius - CeilingHeight;
            Overlay.EndRadius = Ring.RoofRadius;
        }

        return true;
    }
}
