using UnityEngine;
using System.Collections;

public class Window : UIElement
{
    Vector2Int natural_size = new Vector2Int(100, 200);
    Vector2Int natural_position = new Vector2Int(50, 50);

    bool is_grabbed = false;
    Vector2Int grab_offset;

    public CanvasGroup CanvasGroup;
    public RectTransform Handle;

    public bool IsOpen;
    public bool IsMinimized;
    public bool IsMaximized;

    public WindowContainer Windows { get { return GetComponentInParent<WindowContainer>(); } }

    void Update()
    {
        if(!IsOpen)
        {
            if (Windows != null)
                Windows.RemoveWindow(this);

            CanvasGroup.alpha = 0;

            return;
        }
        else
            CanvasGroup.alpha = 1;

        if (Windows == null)
            Scene.The.WindowContainer.AddWindow(this);

        if (InputUtility.WasMouseLeftPressed && Handle.IsPointedAt() && IsTouched)
        {
            is_grabbed = true;

            grab_offset = PixelPosition - Scene.The.Cursor.PixelPointedAt;
        }
        if (InputUtility.WasMouseLeftReleased)
            is_grabbed = false;

        if (is_grabbed)
        {
            Vector2Int new_position = Scene.The.Cursor.PixelPointedAt + grab_offset;
            if(new_position != PixelPosition)
                IsMaximized = false;

            natural_position = new_position;
        }

        if (IsMaximized)
        {
            RectTransform.sizeDelta =
                new Vector2(Scene.The.Style.MonitorResolution.x,
                            Scene.The.Style.MonitorResolution.y - Scene.The.Taskbar.Height);

            RectTransform.anchoredPosition = new Vector2(0, Scene.The.Taskbar.Height);
        }
        else
        {
            RectTransform.sizeDelta =
                new Vector2(natural_size.x,
                            natural_size.y);

            RectTransform.anchoredPosition = new Vector2(natural_position.x,
                                                         natural_position.y);
        }

        if(IsMinimized)
            CanvasGroup.alpha = 0;
    }
}
