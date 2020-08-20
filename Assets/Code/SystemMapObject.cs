using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

[ExecuteAlways]
public class SystemMapObject : UIElement
{
    public CanvasGroup ImageCanvasGroup;
    public RectTransform SatellitesContainer;

    public LineController LineController;
    public TravelingElement OrbitLine;
 
    public string Name;
    public float Mass;
    public float Radius;
    public float Angle;
    public float Apoapsis;
    public float Periapsis;

    public bool IsCircularOrbit = true;

    public float SizeMultiplier = 1;
    public bool AutomaticVisualSize = true;

    public float PositionRealizeSpeed = 16;
    public float SizeChangeSpeed = 3;
    public float AppearSpeed = 3;

    public bool IsFocused
    { get { return Scene.The.SystemMap.FocusedObject == this; } }

    public Vector3 PhysicalPosition
    { get { return PhysicalPositionAtDate(Scene.The.Clock.Now); } }

    public SystemMapObject Primary
    { get { return transform.parent.GetComponentInParent<SystemMapObject>(); } }

    public IEnumerable<SystemMapObject> Satellites
    {
        get
        {
            return SatellitesContainer.GetComponentsInChildren<SystemMapObject>()
                .Where(object_ => object_.Primary == this);
        }
    }

    public SystemMap SystemMap { get { return Scene.The.SystemMap; } }

    void Start()
    {
        LineController.SamplingFunction =
            sample => SystemMap.PhysicalPositionToWorldPosition(
                Primary.PhysicalPosition +
                LocalPhysicalPositionGivenTrueAnomaly(sample * 2 * Mathf.PI))
                .ZChangedTo(Scene.The.Canvas.transform.position.z - 1);

        OrbitLine.Destination = Scene.The._3DUIElementsContainer;
    }

    void Update()
    {
        if (IsCircularOrbit)
            Periapsis = Apoapsis;
        if (Periapsis > Apoapsis)
            Utility.Swap(ref Periapsis, ref Apoapsis);


        gameObject.name = Name;

        int sibling_index = transform.GetSiblingIndex();
        if (sibling_index > 0)
        {
            SystemMapObject sibling =
                transform.parent.GetChild(sibling_index - 1)
                .GetComponent<SystemMapObject>();

            if (sibling.AverageAltitude > AverageAltitude)
                transform.SetAsFirstSibling();
        }


        transform.position = transform.position
            .Lerped(SystemMap.PhysicalPositionToWorldPosition(PhysicalPosition), 
                    PositionRealizeSpeed * Time.deltaTime);

        LineController.Line.enabled = !IsFocused && 
                                      Primary != null && 
                                      ImageCanvasGroup.gameObject.IsTouched();

        float visual_size = SystemMap.FocusedObjectVisualSize;
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

            visual_size = SizeMultiplier *
                          SystemMap.LargestSatelliteVisualSize *
                          normalized_size;

            MaterialPropertyBlock material_property_block = new MaterialPropertyBlock();
            LineController.Line.GetPropertyBlock(material_property_block);

            material_property_block.SetFloat("PathLength", LineController.Length);
            material_property_block.SetVector("ObjectPosition", 
                Scene.The.Canvas.transform.InverseTransformPoint(transform.position));
            material_property_block.SetFloat("ObjectSize", visual_size);

            LineController.Line.SetPropertyBlock(material_property_block);
        }
        RectTransform image_transform = ImageCanvasGroup.transform as RectTransform;
        image_transform.sizeDelta = image_transform.sizeDelta.Lerped(
            visual_size * Vector2.one, SizeChangeSpeed * Time.deltaTime);

        if (!Application.isEditor || Application.isPlaying)
        {
            float target_alpha = 0;

            if (Primary == null ||
                Primary.IsAncestorTo(SystemMap.FocusedObject) ||
                Primary.IsFocused)
            {
                ImageCanvasGroup.blocksRaycasts = true;
                target_alpha = 1;
            }
            else
                ImageCanvasGroup.blocksRaycasts = false;

            ImageCanvasGroup.alpha = 
                Mathf.Lerp(ImageCanvasGroup.alpha, target_alpha, AppearSpeed * Time.deltaTime);
        }
        else
        {
            ImageCanvasGroup.blocksRaycasts = true;
            ImageCanvasGroup.alpha = 1;
        }
    }

    public bool IsAncestorTo(SystemMapObject object_)
    {
        if (object_.Primary == null)
            return false;

        if (object_.Primary == this)
            return true;

        return IsAncestorTo(object_.Primary);
    }

    public bool IsSiblingTo(SystemMapObject object_)
    {
        return Primary == object_.Primary;
    }


    //Derived values

    public float MeanAnomalyAtDate(System.DateTime date)
    {
        return 2 * Mathf.PI *
               Scene.The.Clock.DateToSecondsSinceEpoch(date) /
               Period;
    }

    public float EccentricAnomalyGivenMeanAnomaly(float mean_anomaly)
    {
        return MathUtility.Root(
            x => x - Eccentricity * Mathf.Sin(x) - mean_anomaly,
            x => 1 - Eccentricity * Mathf.Cos(x),
            1e-4f,
            mean_anomaly);

    }

    public float TrueAnomalyGivenMeanAnomaly(float mean_anomaly)
    {
        return 2 * Mathf.Atan(Mathf.Sqrt((1 + Eccentricity) / (1 - Eccentricity)) *
                              Mathf.Tan(EccentricAnomalyGivenMeanAnomaly(mean_anomaly) / 2));
    }

    public float AltitudeGivenTrueAnomaly(float true_anomaly)
    {
        return SemimajorAxis * (1 - Mathf.Pow(Eccentricity, 2)) /
               (1 + Eccentricity * Mathf.Cos(true_anomaly));
    }

    public float VelocityGivenTrueAnomaly(float true_anomaly)
    {
        return Mathf.Sqrt(MathUtility.GravitationalConstant * Primary.Mass *
                          (2 / AltitudeGivenTrueAnomaly(true_anomaly) - 1 / SemimajorAxis));
    }

    public Vector3 LocalPhysicalPositionGivenMeanAnomaly(float mean_anomaly)
    {
        if (Primary == null)
            return Vector3.zero;

        float true_anomaly = TrueAnomalyGivenMeanAnomaly(mean_anomaly);

        return new Vector3(Mathf.Sin(true_anomaly + Angle),
                           Mathf.Cos(true_anomaly + Angle), 0) *
               AltitudeGivenTrueAnomaly(true_anomaly);
    }

    public Vector3 LocalPhysicalPositionGivenTrueAnomaly(float true_anomaly)
    {
        if (Primary == null)
            return Vector3.zero;

        return new Vector3(Mathf.Sin(true_anomaly + Angle),
                           Mathf.Cos(true_anomaly + Angle), 0) *
               AltitudeGivenTrueAnomaly(true_anomaly);
    }

    public Vector3 PhysicalPositionAtDate(System.DateTime date)
    {
        if (Primary == null)
            return Vector3.zero;

        return Primary.PhysicalPositionAtDate(date) + 
               LocalPhysicalPositionGivenMeanAnomaly(MeanAnomalyAtDate(date));
    }


    public float SemimajorAxis
    { get { return (Apoapsis + Periapsis) / 2; } }

    public float SemiminorAxis
    { get { return (Apoapsis + Periapsis) / 2; } }

    public float Eccentricity
    { get { return Apoapsis / SemimajorAxis - 1; } }

    public float Period
    {
        get
        {
            return 2 * Mathf.PI *
                   Mathf.Sqrt(Mathf.Pow(SemimajorAxis, 3) /
                   (MathUtility.GravitationalConstant * Primary.Mass));
        }
    }

    public float AverageAltitude
    { get { return SemimajorAxis * (1 + Mathf.Pow(Eccentricity, 2) / 2); } }

    public float MeanAnomaly
    { get { return MeanAnomalyAtDate(Scene.The.Clock.Now); } }

    public float EccentricAnomaly
    { get { return EccentricAnomalyGivenMeanAnomaly(MeanAnomaly); } }

    public float TrueAnomaly
    { get { return TrueAnomalyGivenMeanAnomaly(MeanAnomaly); } }

    public float Altitude
    { get { return AltitudeGivenTrueAnomaly(TrueAnomaly); } }

    public float Velocity
    { get { return VelocityGivenTrueAnomaly(TrueAnomaly); } }

    public float PathLength
    { get { return MathUtility.EllipseCircumference(SemimajorAxis, Eccentricity); } }
}
