using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StationVisualizationCamera : MonoBehaviour
{
    float start_rail_position;
    float rail_position;
    float rail_transition_moment;

    public Camera Camera;

    public enum ShotType { Establishing, Front, Detail }
    public ShotType Shot = ShotType.Establishing;
    ShotType previous_update_shot;

    public float ShotTransitionDuration = 1;

    public float EstablishingDistance;

    public float NearZoomDistance = 50,
                 FarZoomDistance = 200;
    public float NearPanSpeed = 1;
    public float FarPanSpeed = 1;
    public float Zoom = 0;
    public float ZoomSpeed = 1;
    public float Radians = 0;
    public float Radius = 0;

    public StationVisualization StationVisualization
    { get { return GetComponentInParent<StationVisualization>(); } }

    public RingVisualization FocusedRing { get { return StationVisualization.FocusedRing; } }
    
    private void Start()
    {
        rail_position = 0;
    }

    private void Update()
    {
        //Input

        Zoom = Mathf.Clamp(Zoom + Input.mouseScrollDelta.y * ZoomSpeed, 0, 1);

        float camera_speed = Mathf.Lerp(FarPanSpeed, NearPanSpeed, Zoom);
        if (Input.GetKey(KeyCode.W))
            Radius -= camera_speed * Time.deltaTime;
        if (Input.GetKey(KeyCode.S))
            Radius += camera_speed * Time.deltaTime;
        Radius = Mathf.Clamp(
            Radius, 
            StationVisualization.FocusedRing.Ring.RoofRadius, 
            StationVisualization.FocusedRing.Ring.GroundFloorRadius);

        if (Input.GetKey(KeyCode.D))
            Radians -= camera_speed * Time.deltaTime / Radius;
        if (Input.GetKey(KeyCode.A))
            Radians += camera_speed * Time.deltaTime / Radius;
        if (Radians < 0)
            Radians += 2 * Mathf.PI;
        else if (Radians > 2 * Mathf.PI)
            Radians -= 2 * Mathf.PI;

        if (Input.GetKeyUp(KeyCode.Escape) && Shot != ShotType.Establishing)
            Shot = Shot - 1;


        //Shot transition

        if (previous_update_shot != Shot)
        {
            start_rail_position = rail_position;
            rail_transition_moment = 0;

            previous_update_shot = Shot;
        }

        int transition_count = Utility.GetEnumSize<ShotType>() - 1;
        float target_rail_position = (int)Shot * 1.0f / transition_count;
        rail_transition_moment += Time.deltaTime / ShotTransitionDuration;

        rail_position = Mathf.Lerp(
            start_rail_position, 
            target_rail_position, 
            rail_transition_moment);

        float shot_transition_position = 
            rail_position * transition_count * 0.999f;
        ShotType shot_transition = 
            (ShotType)(shot_transition_position).RoundDown();
        float shot_transition_moment = 
            shot_transition_position % 1 / 0.999f;

        Vector3 starting_position = Vector3.zero,
                ending_position = Vector3.zero;
        Quaternion starting_rotation = Quaternion.identity,
                   ending_rotation = Quaternion.identity;
        switch (shot_transition)
        {
            case ShotType.Establishing:
                starting_position = EstablishingPosition;
                starting_rotation = EstablishingRotation;

                ending_position = FrontPosition;
                ending_rotation = FrontRotation;
                break;

            case ShotType.Front:
                starting_position = FrontPosition;
                starting_rotation = FrontRotation;

                ending_position = DetailPosition;
                ending_rotation = DetailRotation;
                break;
        }

        transform.localPosition =
            starting_position.Lerped(ending_position, shot_transition_moment);
        transform.rotation =
            Quaternion.Lerp(starting_rotation, ending_rotation, shot_transition_moment);


        //RingVisualization

        if (shot_transition == ShotType.Front || shot_transition_moment > 0.5f)
            FocusedRing.WingVisibility = 1;
        else
            FocusedRing.WingVisibility = 0;

        if (shot_transition == ShotType.Front)
        {
            FocusedRing.Linearity = shot_transition_moment * 0.99995f;

            if (Zoom > 0.8f)
                FocusedRing.WireframeVisibility = 1;
            else
                FocusedRing.WireframeVisibility = 0;
        }
        else
            FocusedRing.Linearity = 0;
    }

    public Vector3 EstablishingPosition
    { get { return new Vector3(-1, 1, -1).normalized * EstablishingDistance; } }

    public Quaternion EstablishingRotation
    {
        get
        {
            Vector3 camera_displacement =
                StationVisualization.FocusedRing.transform.localPosition -
                transform.localPosition;

            return Quaternion.LookRotation(
                camera_displacement,
                Vector3.up);
        }
    }

    public Vector3 FrontPosition
    {
        get
        {
            float fov = MathUtility.DegreesToRadians(Camera.fieldOfView);

            return new Vector3(0, 0, -1) *
                   Mathf.Cos(fov / 2) *
                   StationVisualization.FocusedRing.Ring.GroundFloorRadius /
                   Mathf.Sin(fov / 2);
        }
    }

    public Quaternion FrontRotation
    { get { return Quaternion.identity; } }

    public Vector3 DetailPosition
    {
        get
        {
            Vector3 arc_position =
                StationVisualization.FocusedRing.PolarCoordinatesToPosition(
                    Radians,
                    Radius);

            float camera_distance =
                Mathf.Lerp(FarZoomDistance,
                           NearZoomDistance,
                           Zoom);

            return arc_position +
                   new Vector3(0, 0, -camera_distance);
        }
    }

    public Quaternion DetailRotation
    {
        get
        {
            Quaternion rotation;
            StationVisualization.FocusedRing.PolarCoordinatesToPosition(
                    Radians,
                    Radius,
                    out rotation);
            rotation = Quaternion.Euler(0, 0, rotation.eulerAngles.z + 180);

            return rotation;
        }
    }
}
