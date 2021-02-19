using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

[ExecuteAlways]
public class StationVisualization : MonoBehaviour
{
    public Station Station;
    public StationVisualizationCamera Camera;
    
    public IEnumerable<RingVisualization> RingVisualizations
    { get { return GetComponentsInChildren<RingVisualization>(); } }
    public RingVisualization SelectedRing;

    private void Update()
    {
        //Regenerate RingVisualizations if change is detected

        if(RingVisualizations.Count() != Station.Rings.Count())
        {
            foreach (RingVisualization ring_controller in RingVisualizations)
                GameObject.DestroyImmediate(ring_controller.gameObject);

            foreach(Ring ring in Station.Rings)
            {
                RingVisualization ring_visualization = 
                    GameObject.Instantiate(RingVisualizationPrefab);

                ring_visualization.Ring = ring;
                ring_visualization.transform.SetParent(transform);
            }
        }
        if (SelectedRing == null && RingVisualizations.Count() > 0)
            SelectedRing = RingVisualizations.First();


        //Move RingVisualizations into correct positions

        foreach (RingVisualization ring in RingVisualizations)
        {
            float z = RingVisualizations
                .PreviousElements(ring)
                .Sum(ring_ => ring_.Ring.Depth + 50);

            ring.transform.localPosition = new Vector3(0, 0, z);
            ring.Color = Color.green;
        }

        if (The.StationViewer.RectTransform.IsPointedAt(true))
        {
            Ray ray = The.StationViewer.GetRayFromCursorPosition();

            foreach (RingVisualization ring in RingVisualizations)
            {
                if (!ring.Occludes(ray, 0.1f))
                    continue;

                ring.Color = Color.red;

                if (InputUtility.WasMouseLeftReleased)
                {
                    SelectedRing = ring;

                    if (Camera.Shot != StationVisualizationCamera.ShotType.Detail)
                        Camera.Shot =  Camera.Shot + 1;
                }

                break;
            }
        }
    }

    public RingVisualization RingVisualizationPrefab;
}
