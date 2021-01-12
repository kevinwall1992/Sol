using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StationViewer : Page
{
    public RawImage Image;

    public StationVisualization StationVisualization;

    private void Update()
    {
        Camera camera = StationVisualization.Camera.Camera;

        if (camera.targetTexture.width != RectTransform.rect.width || 
            camera.targetTexture.height != RectTransform.rect.height)
        {
            camera.targetTexture.Release();

            Image.texture =
            camera.targetTexture = new RenderTexture(RectTransform.rect.width.Round(), RectTransform.rect.height.Round(), 24);
            camera.targetTexture.filterMode = FilterMode.Point;
        }
    }

    public Ray GetRayFromCursorPosition()
    {
        Vector2 texture_size = new Vector2(
                StationVisualization.Camera.Camera.targetTexture.width,
                StationVisualization.Camera.Camera.targetTexture.height);

        Vector2 image_pixel_pointed_at =
            Scene.The.StationViewer.Image.rectTransform
                .PixelPositionToLocalPosition(Scene.The.Cursor.PixelPointedAt);

        Vector2 normalized_image_position =
            image_pixel_pointed_at /
            Scene.The.StationViewer.Image.rectTransform.rect.size;

        Vector2Int texture_pixel_pointed_at =
            (texture_size * normalized_image_position).Round();

        Ray ray = StationVisualization.Camera.Camera.ScreenPointToRay((Vector2)texture_pixel_pointed_at);
        ray = StationVisualization.transform.InverseTransformRay(ray);

        return ray;
    }
}
