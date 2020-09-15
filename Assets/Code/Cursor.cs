using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Cursor : UIElement
{
    public CanvasGroup CanvasGroup;

    public Image DefaultImage,
                 LeftHorizontalResizeImage, RightHorizontalResizeImage, 
                 VerticalResizeImage,
                 PositiveDiagonalResizeImage, NegativeDiagonalResizeImage;

    public bool IsVisible = true;


    public IEnumerable<GameObject> CanvasElementsPointedAt { get; private set; }
    public GameObject CanvasElementTouched { get; private set; }

    public Vector2Int PixelPointedAt { get { return PixelPosition; } }


    void Start()
    {
        
    }

    void Update()
    {
        UnityEngine.Cursor.visible = false;

        ChangeCursorImage(DefaultImage);

        RectTransform.anchoredPosition = Scene.The.Style.MonitorResolution * 
                                         Input.mousePosition.XY() / 
                                         new Vector2(Screen.width, Screen.height);

        RectTransform.anchoredPosition = 
            new Vector2(Mathf.Min(Mathf.Max(RectTransform.anchoredPosition.x, 0), 
                                  Scene.The.Style.MonitorResolution.x), 
                        Mathf.Min(Mathf.Max(RectTransform.anchoredPosition.y, 0), 
                                  Scene.The.Style.MonitorResolution.y));

        CanvasGroup.alpha = IsVisible ? 1 : 0;



        CanvasElementsPointedAt = Scene.The.Canvas.GetComponentsInChildren<RectTransform>()
            .Where(rect_transform => rect_transform.Contains(
                PixelPointedAt + new Vector2(0.5f, -0.5f),
                Scene.The.UICamera))
            .Select(rect_transform => rect_transform.gameObject).ToList();
               
        
        PointerEventData pointer_event_data = new PointerEventData(null);
        pointer_event_data.position = PixelPointedAt + new Vector2(0.5f, -0.5f);

        List<RaycastResult> raycast_results = new List<RaycastResult>();
        Scene.The.GraphicRaycaster.Raycast(pointer_event_data, raycast_results);

        if (raycast_results.Count == 0)
            CanvasElementTouched = null;
        else
            CanvasElementTouched = raycast_results.First().gameObject;
    }

    public void ChangeCursorImage(Image image)
    {
        if (!image.IsChildOf(this))
            return;

        foreach (Image other in GetComponentsInChildren<Image>())
            other.gameObject.SetActive(false);

        image.gameObject.SetActive(true);
    }
}
