using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ExecuteAlways]
public class StationVisualization : MonoBehaviour
{
    public Station Station;
    public StationVisualizationCamera Camera;
    
    public IEnumerable<RingVisualization> RingVisualizations
    { get { return GetComponentsInChildren<RingVisualization>(); } }
    public RingVisualization FocusedRing;

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
        if (FocusedRing == null && RingVisualizations.Count() > 0)
            FocusedRing = RingVisualizations.First();


        //Move RingVisualizations into correct positions

        int ring_index = 0;
        foreach (RingVisualization ring_visualization in RingVisualizations)
        {
            ring_visualization.transform.localPosition = new Vector3(0, 0, ring_index++ * 500);
        }
    }

    public RingVisualization RingVisualizationPrefab;
}
