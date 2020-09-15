using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

[ExecuteAlways]
public class SatelliteSystemMapObjectController : MonoBehaviour
{
    public SystemMapObject SystemMapObject;
    public Satellite Satellite;

    public float PositionRealizeSpeed = 16;

    public SystemMap SystemMap { get { return Scene.The.SystemMap; } }

    void Start()
    {
        if(Satellite.Primary != null)
            SystemMapObject.LineController.SamplingFunction = sample =>
               Scene.The.SystemMap.PhysicalPositionToWorldPosition(
                   Satellite.Primary.Position +
                   Satellite.Motion.LocalPositionGivenTrueAnomaly(sample * 2 * Mathf.PI))
               .ZChangedTo(Scene.The.Canvas.transform.position.z - 1);
    }

    void Update()
    {
        SystemMapObject.VisualSize = GetVisualSize();


        if (Satellite.Primary == null)
            SystemMapObject.Parent = null;
        else
            SystemMapObject.Parent = Satellite.Primary.GetComponent<SystemMapObject>();


        Vector3 physical_position = Satellite.Motion.PositionAtDate(Scene.The.Clock.Now);

        SystemMapObject.transform.position = SystemMapObject.transform.position
            .Lerped(SystemMap.PhysicalPositionToWorldPosition(physical_position),
                    PositionRealizeSpeed * Time.deltaTime);
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
                    Scene.The.SystemMap.SmallestNaturalSatelliteVisualSize /
                    Scene.The.SystemMap.LargestNaturalSatelliteVisualSize;
            else
                smallest_normalized_size =
                    Scene.The.SystemMap.SmallestArtificialSatelliteVisualSize /
                    Scene.The.SystemMap.LargestArtificialSatelliteVisualSize;

            relative_size =
                (smallest_normalized_size - 1) *
                Mathf.Log(Satellite.Radius / largest_radius) /
                Mathf.Log(smallest_radius / largest_radius) +
                1;
        }

        return relative_size *
               (Satellite.IsNaturalSatellite() ?
                Scene.The.SystemMap.LargestNaturalSatelliteVisualSize :
                Scene.The.SystemMap.LargestArtificialSatelliteVisualSize);
    }
}
