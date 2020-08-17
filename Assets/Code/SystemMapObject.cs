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
    public float Altitude;

    public float SizeMultiplier = 1;
    public bool AutomaticVisualSize = true;

    public bool IsFocused
    { get { return Scene.The.SystemMap.FocusedObject == this; } }

    public float Velocity
    {
        get
        {
            return Mathf.Sqrt(MathUtility.GravitationalConstant * 
                              Primary.Mass / 
                              Altitude);
        }
    }

    public float AngularVelocity
    { get { return Velocity / Altitude; } }

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
        gameObject.name = Name;

        int sibling_index = transform.GetSiblingIndex();
        if (sibling_index > 0)
        {
            SystemMapObject sibling = 
                transform.parent.GetChild(sibling_index - 1)
                .GetComponent<SystemMapObject>();

            if(sibling.Altitude > Altitude)
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


        if (Primary == null)
            return;

        if (Primary == SystemMap.FocusedObject)
            transform.localPosition =
                Quaternion.Euler(0, 0, MathUtility.RadiansToDegrees(
                    AngularVelocity * Scene.The.Clock.Seconds)) *
                new Vector3(SystemMap.GetPixelSize(Altitude), 0, 0);
        else
            transform.localPosition = new Vector3(0, 0, 0);
    }
}
