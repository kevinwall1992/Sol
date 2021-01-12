using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class RingVisualizationOverlay : MonoBehaviour
{
    public float StartRadians, EndRadians, 
                 StartRadius, EndRadius;

    public float DegreesPerSample = 5;

    public RingVisualization RingVisualization
    { get { return Scene.The.StationViewer.StationVisualization.SelectedRing; } }

    public LineRenderer LineRenderer
    { get { return GetComponent<LineRenderer>(); } }

    private void Update()
    {
        if (RingVisualization == null)
            return;
        if (transform.parent != RingVisualization.transform)
            transform.SetParent(RingVisualization.transform, false);

        if (StartRadians > EndRadians)
            Utility.Swap(ref StartRadians, ref EndRadians);
        if (StartRadius > EndRadius)
            Utility.Swap(ref StartRadius, ref EndRadius);


        float radians = EndRadians - StartRadians;
        float thickness = EndRadius - StartRadius;

        int resolution =
            (radians / 
            MathUtility.DegreesToRadians(DegreesPerSample))
            .Round();
        resolution = Mathf.Max(resolution, 2);

        LineRenderer.positionCount = resolution + 1;
        LineRenderer.startWidth =
        LineRenderer.endWidth = thickness;

        for (int i = 0; i <= resolution; i++)
        {
            float progress = (float)i / resolution;
            Vector3 position = RingVisualization.PolarCoordinatesToPosition(
                StartRadians + progress * radians,
                StartRadius + thickness / 2);

            LineRenderer.SetPosition(i, position);
        }
    }

    public void SetParameters(float start_radians, float end_radians,
                              float start_radius, float end_radius, 
                              Material material = null)
    {
        StartRadians = start_radians;
        EndRadians = end_radians;

        StartRadius = start_radius;
        EndRadius = end_radius;

        if (material != null)
            LineRenderer.material = material;
    }
}
