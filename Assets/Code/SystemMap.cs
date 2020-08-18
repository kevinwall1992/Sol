using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ExecuteAlways]
public class SystemMap : Page
{
    float nearest_satellite_distance, 
          furthest_satellite_distance;

    public RectTransform ObjectsContainer;
    public SystemMapObject Sun;

    public float FocusedObjectVisualSize = 30;
    public float SmallestSatelliteVisualSize = 3,
                 LargestSatelliteVisualSize = 20;

    SystemMapObject focused_object;
    public SystemMapObject FocusedObject
    {
        get { return focused_object; }
        set
        {
            if (value == null)
                focused_object = Sun;
            else
                focused_object = value;

            SystemMapObject reference = FocusedObject;
            if (reference.Satellites.Count() == 0)
                reference = FocusedObject.Primary;

            nearest_satellite_distance = reference.Satellites.Min(satellite => satellite.Periapsis);
            furthest_satellite_distance = reference.Satellites.Max(satellite => satellite.Apoapsis);
        }
    }

    private void Start()
    {
        FocusedObject = Sun;
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
            FocusedObject = FocusedObject.Primary;
        if (FocusedObject == null)
            FocusedObject = Sun;
    }

    float GetDistanceInPixels(float distance_in_meters)
    {
        float smallest_dimension = Mathf.Min(RectTransform.rect.width,
                                             RectTransform.rect.height);

        float normalized_distance = 1;
        if (FocusedObject.Satellites.Count() > 1)
        {
            float smallest_normalized_distance =
                (FocusedObjectVisualSize + LargestSatelliteVisualSize) /
                smallest_dimension;

            normalized_distance =
                (smallest_normalized_distance - 1) *
                Mathf.Log(distance_in_meters / furthest_satellite_distance) /
                Mathf.Log(nearest_satellite_distance / furthest_satellite_distance) +
                1;
        }

        return (smallest_dimension / 2) * 0.95f * normalized_distance;
    }

    public Vector2 SolarPositionToWorldPosition(Vector3 solar_position)
    {
        Vector3 displacement = solar_position - FocusedObject.SolarPosition;

        if (displacement.magnitude == 0)
            return ObjectsContainer.position;

        return ObjectsContainer.TransformPoint(
            displacement.XY().normalized *
            GetDistanceInPixels(displacement.magnitude));
    }
}
