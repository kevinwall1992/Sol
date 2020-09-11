using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;


[ExecuteAlways]
public class SystemMapObject : UIElement
{
    public SatelliteMotion Motion;

    public CanvasGroup ImageCanvasGroup;
    public RectTransform SatellitesContainer;

    public LineController LineController;
    public TravelingElement OrbitLine;
 
    public string Name;
    public float Mass;
    public float Radius;

    public float VisualSize = 20;
    public float FocusedVisualSize = 40;
    public float SizeMultiplier = 1;
    public bool AutomaticVisualSize = true;

    public float PositionRealizeSpeed = 16;
    public float SizeChangeSpeed = 3;
    public float AppearSpeed = 3;

    public bool IsFocused
    { get { return Scene.The.SystemMap.FocusedObject == this; } }

    public Vector3 PhysicalPosition
    { get { return Motion.PositionAtDate(Scene.The.Clock.Now); } }

    public float Altitude { get { return Motion.DistanceFromPrimary; } }
    public Vector3 Velocity { get { return Motion.Velocity; } }

    public SystemMapObject Primary
    { get { return Motion.Primary; } }

    public IEnumerable<SystemMapObject> Satellites
    {
        get
        {
            return SatellitesContainer.GetComponentsInChildren<SystemMapObject>()
                .Where(object_ => object_.Primary == this);
        }
    }

    public SystemMap SystemMap { get { return Scene.The.SystemMap; } }

    protected virtual void Start()
    {
        Motion.Primary = transform.parent.GetComponentInParent<SystemMapObject>();

        LineController.SamplingFunction =
            sample => SystemMap.PhysicalPositionToWorldPosition(
                Primary.PhysicalPosition +
                Motion.LocalPositionGivenTrueAnomaly(sample * 2 * Mathf.PI))
                .ZChangedTo(Scene.The.Canvas.transform.position.z - 1);

        OrbitLine.Destination = Scene.The._3DUIElementsContainer;
    }

    protected virtual void Update()
    {
        UpdateParent();

        if (Motion.Periapsis > Motion.Apoapsis)
            Utility.Swap(ref Motion.Periapsis, ref Motion.Apoapsis);


        if (gameObject.name != Name)
            gameObject.name = Name;

        int sibling_index = transform.GetSiblingIndex();
        if (sibling_index > 0)
        {
            SystemMapObject sibling =
                transform.parent.GetChild(sibling_index - 1)
                .GetComponent<SystemMapObject>();

            if (sibling.Motion.Periapsis > Motion.Periapsis)
                transform.SetAsFirstSibling();
        }


        transform.position = transform.position
            .Lerped(SystemMap.PhysicalPositionToWorldPosition(PhysicalPosition), 
                    PositionRealizeSpeed * Time.deltaTime);

        LineController.Line.enabled = !IsFocused && 
                                      Primary != null && 
                                      ImageCanvasGroup.gameObject.IsTouched();

        if (!IsFocused && Primary != null)
        {
            float largest_radius = Primary.Satellites.Max(satellite => satellite.Radius);
            float smallest_radius = Primary.Satellites.Min(satellite => satellite.Radius);

            float normalized_size = 1;
            if (Primary.Satellites.Count() > 1)
            {
                float smallest_normalized_size =
                    SystemMap.SmallestSatelliteVisualSize /
                    SystemMap.LargestSatelliteVisualSize;

                normalized_size =
                    (smallest_normalized_size - 1) *
                    Mathf.Log(Radius / largest_radius) /
                    Mathf.Log(smallest_radius / largest_radius) +
                    1;
            }

            if (AutomaticVisualSize)
                VisualSize = SizeMultiplier *
                             SystemMap.LargestSatelliteVisualSize *
                             normalized_size;

            MaterialPropertyBlock material_property_block = new MaterialPropertyBlock();
            LineController.Line.GetPropertyBlock(material_property_block);

            material_property_block.SetFloat("PathLength", LineController.Length);
            material_property_block.SetVector("ObjectPosition", 
                Scene.The.Canvas.transform.InverseTransformPoint(transform.position));
            material_property_block.SetFloat("ObjectSize", VisualSize);

            LineController.Line.SetPropertyBlock(material_property_block);
        }
        RectTransform image_transform = ImageCanvasGroup.transform as RectTransform;
        float target_visual_size = IsFocused ? FocusedVisualSize : VisualSize;
        image_transform.sizeDelta = image_transform.sizeDelta.Lerped(
            target_visual_size * Vector2.one, SizeChangeSpeed * Time.deltaTime);

        if (!Application.isEditor || Application.isPlaying)
        {
            float target_alpha = 0;

            if (Primary == null ||
                SystemMap.FocusedObject.IsChildOf(Primary) || 
                Primary.IsFocused)
            {
                ImageCanvasGroup.blocksRaycasts = true;
                target_alpha = 1;
            }
            else
                ImageCanvasGroup.blocksRaycasts = false;

            ImageCanvasGroup.alpha = 
                Mathf.Lerp(ImageCanvasGroup.alpha, 
                           target_alpha, 
                           AppearSpeed * Time.deltaTime);
        }
        else
        {
            ImageCanvasGroup.blocksRaycasts = true;
            ImageCanvasGroup.alpha = 1;
        }
    }

    void UpdateParent()
    {
        if (Primary != null && 
            transform.parent != Motion.Primary.SatellitesContainer)
            transform.SetParent(Primary.SatellitesContainer);
    }

    public void ChangeMotion(SatelliteMotion motion)
    {
        Motion = motion;

        UpdateParent();
    }
}


public static class SystemMapObjectExtensions
{
    public static Craft Craft(this SystemMapObject object_)
    {
        return object_.GetComponent<Craft>();
    }

    public static bool IsCraft(this SystemMapObject object_)
    {
        return object_.Craft() != null;
    }

    public static Station Station(this SystemMapObject object_)
    {
        return object_.GetComponent<Station>();
    }

    public static bool IsStation(this SystemMapObject object_)
    {
        return object_.Station() != null;
    }

    public static Visitable Place(this SystemMapObject object_)
    {
        if (object_.HasComponent<Station>())
            return object_.GetComponent<Station>();
        else if (object_.HasComponent<NaturalBody>())
            return object_.GetComponent<NaturalBody>();

        return null;
    }

    public static bool IsVisitable(this SystemMapObject object_)
    {
        return object_.Place() != null;
    }
}
