using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Cursor : UIElement
{
    public Vector2Int PixelPointedAt { get { return PixelPosition; } }

    void Start()
    {
        
    }

    void Update()
    {
        UnityEngine.Cursor.visible = false;

        RectTransform.anchoredPosition = Scene.The.Style.MonitorResolution * 
                                         Input.mousePosition.XY() / 
                                         new Vector2(Screen.width, Screen.height);

        RectTransform.anchoredPosition = 
            new Vector2(Mathf.Min(Mathf.Max(RectTransform.anchoredPosition.x, 0), 
                                  Scene.The.Style.MonitorResolution.x), 
                        Mathf.Min(Mathf.Max(RectTransform.anchoredPosition.y, 0), 
                                  Scene.The.Style.MonitorResolution.y));
    }
}
