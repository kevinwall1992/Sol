using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ExecuteAlways]
public class SystemMap : Page
{
    public RectTransform ObjectsContainer;
    public SystemMapObject Sun;

    public float FocusedObjectVisualSize = 30;
    public float SmallestSatelliteVisualSize = 3,
                 LargestSatelliteVisualSize = 20;

    public SystemMapObject FocusedObject { get; set; }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
            FocusedObject = FocusedObject.Primary;
        if (FocusedObject == null)
            FocusedObject = Sun;

        RectTransform objects_container_container = (ObjectsContainer.parent as RectTransform);

        ObjectsContainer.position =
            objects_container_container.TransformPoint(objects_container_container.rect.size / 2) + 
            (ObjectsContainer.position - FocusedObject.transform.position);
    }

    public float GetPixelSize(float meters)
    {
        float smallest_dimension = Mathf.Min(RectTransform.rect.width,
                                             RectTransform.rect.height);

        SystemMapObject reference = FocusedObject;
        if (reference.Satellites.Count() == 0)
            reference = FocusedObject.Primary;

        float largest_distance = reference.Satellites.Max(satellite => satellite.Altitude);
        float smallest_distance = reference.Satellites.Min(satellite => satellite.Altitude);

        float normalized_distance = 1;
        if (FocusedObject.Satellites.Count() > 1)
        {
            float smallest_normalized_distance = 
                (FocusedObjectVisualSize + LargestSatelliteVisualSize) / 
                smallest_dimension;

            normalized_distance =
                (smallest_normalized_distance - 1) *
                Mathf.Log(meters / largest_distance) /
                Mathf.Log(smallest_distance / largest_distance) + 
                1;
        }

        return (smallest_dimension / 2) * 0.95f * normalized_distance;
    }
}
