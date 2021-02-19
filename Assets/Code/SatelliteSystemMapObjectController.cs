using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

[ExecuteAlways]
public class SatelliteSystemMapObjectController : MonoBehaviour
{
    Vector3 target_position;

    public SystemMapObject SystemMapObject;
    public Satellite Satellite;

    public float PositionRealizeSpeed = 16;

    public SystemMap SystemMap { get { return The.SystemMap; } }

    void Start()
    {
        if(Satellite.Primary != null)
            SystemMapObject.LineController.SamplingFunction = sample =>
               The.SystemMap.PhysicalPositionToWorldPosition(
                   Satellite.Primary.Position +
                   Satellite.Motion.LocalPositionGivenTrueAnomaly(sample * 2 * Mathf.PI))
               .ZChangedTo(The.Canvas.transform.position.z - 1);
    }

    void Update()
    {
        SystemMapObject.VisualSize = GetVisualSize();


        if (Satellite.Primary == null)
            SystemMapObject.Parent = null;
        else
            SystemMapObject.Parent = Satellite.Primary.GetComponent<SystemMapObject>();

        if (this.IsModulusUpdate(8))
        {
            Vector3 physical_position = Satellite.Motion.PositionAtDate(The.Clock.Now);
            target_position = SystemMap.PhysicalPositionToWorldPosition(physical_position);
        }

        SystemMapObject.transform.position = SystemMapObject.transform.position
                .Lerped(target_position, PositionRealizeSpeed * Time.deltaTime);
    }

    float GetVisualSize()
    {
        if (Satellite.Primary == null)
            return SystemMap.RootVisualSize;

        if (SystemMapObject.IsFocused)
            return SystemMap.FocusedObjectVisualSize;

        Satellite primary = (Satellite.Motion as SatelliteMotion).Primary;

        IEnumerable<Satellite> satellites;
        if (Satellite.IsNaturalSatellite())
            satellites = primary.Satellites
                .Where(satellite => satellite.IsNaturalSatellite());
        else
            satellites = primary.Satellites
                .Where(satellite => !satellite.IsNaturalSatellite());

        float largest_radius = satellites.Max(satellite => satellite.Radius);
        float smallest_radius = satellites.Min(satellite => satellite.Radius);

        float relative_size = 1;
        if (satellites.Count() > 1)
        {
            float smallest_normalized_size;
            if (Satellite.IsNaturalSatellite())
                smallest_normalized_size =
                    The.SystemMap.SmallestNaturalSatelliteVisualSize /
                    The.SystemMap.LargestNaturalSatelliteVisualSize;
            else
                smallest_normalized_size =
                    The.SystemMap.SmallestArtificialSatelliteVisualSize /
                    The.SystemMap.LargestArtificialSatelliteVisualSize;

            relative_size =
                (smallest_normalized_size - 1) *
                Mathf.Log(Satellite.Radius / largest_radius) /
                Mathf.Log(smallest_radius / largest_radius) +
                1;
        }

        return relative_size *
               (Satellite.IsNaturalSatellite() ?
                The.SystemMap.LargestNaturalSatelliteVisualSize :
                The.SystemMap.LargestArtificialSatelliteVisualSize);
    }
}
