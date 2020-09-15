using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ExecuteAlways]
public class SystemMap : Page
{
    public RectTransform ObjectsContainer;
    public SystemMapObject Root;

    public TransportCraftPanel TransportCraftPanel;
    public PlacePanel PlacePanel;

    public Material OrbitLineMaterial;

    public float SmallestNaturalSatelliteVisualSize = 3,
                 LargestNaturalSatelliteVisualSize = 20,
                 SmallestArtificialSatelliteVisualSize = 3,
                 LargestArtificialSatelliteVisualSize = 20,
                 FocusedObjectVisualSize = 30,
                 RootVisualSize = 30;

    public float InnermostOrbitMargin = 20;

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

    public IEnumerable<SystemMapObject> Objects
    { get { return GetComponentsInChildren<SystemMapObject>(); } }

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
                focused_object = Root;
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
        FocusedObject = Root;
        last_focused_object = Root;
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
            FocusedObject = FocusedObject.Parent;
        if (FocusedObject == null)
            FocusedObject = Root;
        if (last_focused_object == null)
            last_focused_object = Root;


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

    float GetCompressedDistance(Satellite reference, float distance_in_meters)
    {
        float innermost_satellite_visual_size = 0;
        if (reference.Satellites.Count() > 0)
            innermost_satellite_visual_size = reference.Satellites
                .MinElement(satellite => satellite.Motion.Periapsis)
                    .SystemMapObject().VisualSize;

        float smallest_normalized_distance =
            (FocusedObject.VisualSize + 
             innermost_satellite_visual_size +
             InnermostOrbitMargin) /
            (2 * PixelRadius);
        if (reference.SystemMapObject() != Root)
            smallest_normalized_distance *= 1.4f;

        float nearest_distance;
        float furthest_distance;
        if (reference.Satellites.Count() > 0)
        {
            nearest_distance = 
                reference.Satellites.Min(satellite => satellite.Motion.Periapsis);
            furthest_distance = 
                reference.Satellites.Max(satellite => satellite.Motion.Apoapsis);
        }
        else
        {
            nearest_distance = 10 * reference.Radius;
            furthest_distance = 100 * reference.Radius;
        }

        if (nearest_distance == furthest_distance)
            nearest_distance = furthest_distance / 10;

        float compressed_distance =
            (smallest_normalized_distance - 1) *
            Mathf.Log(distance_in_meters / furthest_distance) /
            Mathf.Log(nearest_distance / furthest_distance) +
            1;

        return compressed_distance;
    }

    public Vector3 GetCompressedPosition(Satellite reference, 
                                         Vector3 physical_position)
    {
        Vector3 displacement = physical_position - reference.Position;
        if (displacement.magnitude == 0)
            return Vector3.zero;

        return displacement.XY().normalized *
               GetCompressedDistance(reference, displacement.magnitude);
    }

    public Vector3 PhysicalPositionToWorldPosition(Vector3 physical_position)
    {
        Satellite root_satellite = Root.GetComponent<Satellite>();
        Satellite focused_satellite = FocusedObject.GetComponent<Satellite>();
        Satellite last_focused_satellite = last_focused_object.GetComponent<Satellite>();
        if (focused_satellite == null || 
            last_focused_satellite == null || 
            root_satellite == null)
            throw new System.NotImplementedException();

        Vector3 compressed_position = 
            GetCompressedPosition(focused_satellite, physical_position);

        if (TransitionMoment < 1)
            compressed_position = compressed_position.Lerped(
                GetCompressedPosition(last_focused_satellite, physical_position), 
                1 - TransitionMoment);

        compressed_position = compressed_position.Lerped(
            GetCompressedPosition(root_satellite, physical_position), 
            0.5f);

        return ObjectsContainer.TransformPoint(compressed_position * PixelRadius);
    }
}
