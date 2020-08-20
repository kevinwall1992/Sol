using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ExecuteAlways]
public class SystemMap : Page
{
    public RectTransform ObjectsContainer;
    public SystemMapObject Sun;

    public Material OrbitLineMaterial;

    public float FocusedObjectVisualSize = 30;
    public float SmallestSatelliteVisualSize = 3,
                 LargestSatelliteVisualSize = 20;

    public float TransitionDuration = 5;
    public float TransitionMoment
    {
        get
        {
            return Mathf.Min(1,
                (float)(System.DateTime.Now - focus_change_timestamp).TotalSeconds /
                TransitionDuration);
        }
    }

    SystemMapObject focused_object, last_focused_object;
    System.DateTime focus_change_timestamp;
    public SystemMapObject FocusedObject
    {
        get { return focused_object; }
        set
        {
            last_focused_object = FocusedObject;
            focus_change_timestamp = System.DateTime.Now;

            if (value == null)
                focused_object = Sun;
            else
                focused_object = value;
        }
    }

    public float PixelRadius
    {
        get
        {
            return 0.95f / 2 * Mathf.Min(ObjectsContainerContainer.rect.width,
                                         ObjectsContainerContainer.rect.height);
        }
    }

    RectTransform ObjectsContainerContainer
    { get { return ObjectsContainer.parent as RectTransform; } }

    private void Start()
    {
        FocusedObject = Sun;
        last_focused_object = Sun;
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
            FocusedObject = FocusedObject.Primary;
        if (FocusedObject == null)
            FocusedObject = Sun;


        RectTransform canvas_transform = Scene.The.Canvas.transform as RectTransform;
        Vector2 screen_space_min = canvas_transform
            .InverseTransformPoint(RectTransform
            .TransformPoint(RectTransform.rect.min)).XY();
        Vector2 screen_space_max = canvas_transform
            .InverseTransformPoint(RectTransform
            .TransformPoint(RectTransform.rect.max)).XY();

        OrbitLineMaterial.SetVector("MaskMin", screen_space_min);
        OrbitLineMaterial.SetVector("MaskMax", screen_space_max);


        ObjectsContainer.localPosition = ObjectsContainerContainer.rect.size / 2;
    }

    float GetCompressedDistance(SystemMapObject reference, float distance_in_meters)
    {
        float compressed_distance = 1;
        if (reference.Satellites.Count() > 1)
        {
            float smallest_normalized_distance =
                (FocusedObjectVisualSize + LargestSatelliteVisualSize) /
                (2 * PixelRadius);

            float nearest_satellite_distance = 
                reference.Satellites.Min(satellite => satellite.Periapsis);
            float furthest_satellite_distance = 
                reference.Satellites.Max(satellite => satellite.Apoapsis);

            compressed_distance =
                (smallest_normalized_distance - 1) *
                Mathf.Log(distance_in_meters / furthest_satellite_distance) /
                Mathf.Log(nearest_satellite_distance / furthest_satellite_distance) +
                1;
        }

        return compressed_distance;
    }

    public Vector3 GetCompressedPosition(SystemMapObject reference, 
                                         Vector3 physical_position)
    {
        Vector3 displacement = physical_position - reference.PhysicalPosition;
        if (displacement.magnitude == 0)
            return Vector3.zero;

        return displacement.XY().normalized *
               GetCompressedDistance(reference, displacement.magnitude);
    }

    public Vector3 PhysicalPositionToWorldPosition(Vector3 physical_position)
    {
        Vector3 compressed_position = GetCompressedPosition(FocusedObject, physical_position);

        if (TransitionMoment < 1)
            compressed_position = compressed_position.Lerped(
                GetCompressedPosition(last_focused_object, physical_position), 
                1 - TransitionMoment);

        compressed_position = compressed_position.Lerped(
            GetCompressedPosition(Sun, physical_position), 
            0.5f);

        return ObjectsContainer.TransformPoint(compressed_position * PixelRadius);
    }
}
