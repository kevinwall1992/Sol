using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

[ExecuteAlways]
public class SystemMapObject : MonoBehaviour
{
    public Image Image;
    public RectTransform SatellitesContainer;

    public string Name;
    public float Mass;
    public float Radius;
    public float Angle;
    public float Apoapsis;
    public float Periapsis;

    public bool IsCircularOrbit = true;

    public float SizeMultiplier = 1;
    public bool AutomaticVisualSize = true;

    public bool IsFocused
    { get { return Scene.The.SystemMap.FocusedObject == this; } }

    public Vector3 SolarPosition
    { get { return GetSolarPositionAtDate(Scene.The.Clock.Now); } }

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


        float visual_size = 0;
        if (IsFocused)
            visual_size = SystemMap.FocusedObjectVisualSize;
        else if (Primary != null)
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
        }
        (Image.transform as RectTransform).sizeDelta = visual_size * Vector2.one;

        if (!Application.isEditor || Application.isPlaying)
            Image.gameObject.SetActive(SystemMap.FocusedObject == this ||
                                       SystemMap.FocusedObject == Primary);
        else
            Image.gameObject.SetActive(true);


        transform.position = SystemMap.SolarPositionToWorldPosition(SolarPosition);
    }

    public float GetMeanAnomalyAtDate(System.DateTime date)
    {
        return 2 * Mathf.PI * 
               Scene.The.Clock.DateToSecondsSinceEpoch(date) / 
               Period;
    }

    public float GetEccentricAnomalyAtDate(System.DateTime date)
    {
        float mean_anomaly_at_date = GetMeanAnomalyAtDate(date);

        return MathUtility.Root(
            x => x - Eccentricity * Mathf.Sin(x) - mean_anomaly_at_date,
            x => 1 - Eccentricity * Mathf.Cos(x),
            1e-4f,
            mean_anomaly_at_date);

    }

    public float GetTrueAnomalyAtDate(System.DateTime date)
    {
        return 2 * Mathf.Atan(Mathf.Sqrt((1 + Eccentricity) / (1 - Eccentricity)) *
                              Mathf.Tan(GetEccentricAnomalyAtDate(date) / 2));

    }

    public float GetAltitudeAtDate(System.DateTime date)
    {
        return SemimajorAxis * (1 - Eccentricity * Mathf.Cos(GetEccentricAnomalyAtDate(date)));
    }

    public Vector3 GetSolarPositionAtDate(System.DateTime date)
    {
        if (Primary == null)
            return Vector3.zero;

        float true_anomaly_at_date = GetTrueAnomalyAtDate(date);

        return Primary.GetSolarPositionAtDate(date) +
               new Vector3(Mathf.Sin(true_anomaly_at_date + Angle),
                           Mathf.Cos(true_anomaly_at_date + Angle), 0) * 
               GetAltitudeAtDate(date);
    }


    //Derived values

    public float SemimajorAxis
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
    { get { return GetMeanAnomalyAtDate(Scene.The.Clock.Now); } }

    public float EccentricAnomaly
    { get { return GetEccentricAnomalyAtDate(Scene.The.Clock.Now); } }

    public float TrueAnomaly
    { get { return GetTrueAnomalyAtDate(Scene.The.Clock.Now); } }

    public float Altitude
    { get { return GetAltitudeAtDate(Scene.The.Clock.Now); } }
}
