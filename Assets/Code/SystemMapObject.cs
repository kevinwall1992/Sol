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
    {
        get
        {
            if (Primary == null)
                return Vector3.zero;

            return Primary.SolarPosition + 
                   new Vector3(Mathf.Sin(TrueAnomaly + Angle), 
                               Mathf.Cos(TrueAnomaly + Angle), 0) * Altitude;
        }
    }

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

            if(sibling.AverageAltitude > AverageAltitude)
                transform.SetAsFirstSibling();
        }


        float visual_size = 0;
        if (IsFocused)
            visual_size = SystemMap.FocusedObjectVisualSize;
        else if(Primary != null)
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
    { get { return 2 * Mathf.PI * Scene.The.Clock.Seconds / Period; } }

    public float EccentricAnomaly
    {
        get
        {
            return MathUtility.Root(
                x => x - Eccentricity * Mathf.Sin(x) - MeanAnomaly,
                x => 1 - Eccentricity * Mathf.Cos(x),
                1e-4f,
                MeanAnomaly);
        }
    }

    public float TrueAnomaly
    {
        get
        {
            return 2 * Mathf.Atan(Mathf.Sqrt((1 + Eccentricity) / (1 - Eccentricity)) *
                                  Mathf.Tan(EccentricAnomaly / 2));
        }
    }

    public float Altitude
    { get { return SemimajorAxis * (1 - Eccentricity * Mathf.Cos(EccentricAnomaly)); } }
}
