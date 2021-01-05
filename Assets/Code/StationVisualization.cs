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
        foreach (RingVisualization ring in RingVisualizations)
        {
            ring.transform.localPosition = new Vector3(0, 0, ring_index++ * 500);
            ring.Color = Color.green;

            foreach (WingVisualization wing in ring.WingVisualizations)
                wing.Color = Color.yellow;
        }

        if (Scene.The.StationViewer.RectTransform.IsPointedAt(true))
        {
            Vector2 texture_size = new Vector2(
                Camera.Camera.activeTexture.width, 
                Camera.Camera.activeTexture.height);

            Vector2 image_pixel_pointed_at = 
                Scene.The.StationViewer.Image.rectTransform
                    .PixelPositionToLocalPosition(Scene.The.Cursor.PixelPointedAt);

            Vector2 normalized_image_position =
                image_pixel_pointed_at /
                Scene.The.StationViewer.Image.rectTransform.rect.size;

            Vector2Int texture_pixel_pointed_at = 
                (texture_size * normalized_image_position).Round();

            Ray ray = Camera.Camera.ScreenPointToRay((Vector2)texture_pixel_pointed_at);
            ray = transform.InverseTransformRay(ray);

            foreach (RingVisualization ring in RingVisualizations)
            {
                if (!ring.Occludes(ray, 0.1f))
                    continue;

                ring.Color = Color.red;

                Vector2 polar_coordinates = ring.PolarCoordinatesFromRay(ray);

                foreach (WingVisualization wing_visualization in ring.WingVisualizations)
                {
                    Ring.Floor.Wing wing = wing_visualization.Wing;

                    if (polar_coordinates.x >= (wing.Radians - wing.RadianWidth / 2) && 
                        polar_coordinates.x < (wing.Radians + wing.RadianWidth / 2) && 
                        polar_coordinates.y <= wing.Floor.Radius && 
                        polar_coordinates.y > (wing.Floor.Radius - wing.Floor.CeilingHeight))
                    {
                        wing_visualization.Color = Color.red;

                        if (InputUtility.WasMouseLeftReleased && 
                            Camera.Shot == StationVisualizationCamera.ShotType.Front)
                        {
                            Camera.Shot = StationVisualizationCamera.ShotType.Detail;
                            Camera.Radians = wing.Radians;
                        }

                        break;
                    }
                }

                if (InputUtility.WasMouseLeftReleased)
                {
                    FocusedRing = ring;

                    if (Camera.Shot == StationVisualizationCamera.ShotType.Establishing)
                        Camera.Shot = StationVisualizationCamera.ShotType.Front;
                }

                break;
            }
        }
    }

    public RingVisualization RingVisualizationPrefab;
}
