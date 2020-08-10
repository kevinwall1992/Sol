using UnityEngine;
using System.Collections;

public class Window : UIElement
{
    Vector2Int natural_size = new Vector2Int(100, 200);
    Vector2Int natural_position = new Vector2Int(50, 50);

    bool is_grabbed = false;
    Vector2Int grab_offset;

    enum ResizeGrabType { None, Left, BottomLeft, Bottom, BottomRight, Right }
    ResizeGrabType resize_grab_type = ResizeGrabType.None;

    public CanvasGroup CanvasGroup;

    public RectTransform Handle, 
                         LeftEdge, 
                         BottomLeftCorner, 
                         BottomEdge, 
                         BottomRightCorner, 
                         RightEdge;

    public bool IsOpen;
    public bool IsMinimized;
    public bool IsMaximized;

    public Vector2Int MinimumSize;

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

        if (!is_grabbed)
        {
            resize_grab_type = ResizeGrabType.None;
            if (!IsMaximized && IsTouched)
            {
                if (LeftEdge.IsPointedAt())
                    resize_grab_type = ResizeGrabType.Left;
                else if (BottomLeftCorner.IsPointedAt())
                    resize_grab_type = ResizeGrabType.BottomLeft;
                else if (BottomEdge.IsPointedAt())
                    resize_grab_type = ResizeGrabType.Bottom;
                else if (BottomRightCorner.IsPointedAt())
                    resize_grab_type = ResizeGrabType.BottomRight;
                else if (RightEdge.IsPointedAt())
                    resize_grab_type = ResizeGrabType.Right;
            }
        }
        switch (resize_grab_type)
        {
            case ResizeGrabType.Left:
                Scene.The.Cursor.ChangeCursorImage(Scene.The.Cursor.LeftHorizontalResizeImage);
                break;
            case ResizeGrabType.Right:
                Scene.The.Cursor.ChangeCursorImage(Scene.The.Cursor.RightHorizontalResizeImage);
                break;
            case ResizeGrabType.BottomLeft:
                Scene.The.Cursor.ChangeCursorImage(Scene.The.Cursor.PositiveDiagonalResizeImage);
                break;
            case ResizeGrabType.BottomRight:
                Scene.The.Cursor.ChangeCursorImage(Scene.The.Cursor.NegativeDiagonalResizeImage);
                break;
            case ResizeGrabType.Bottom:
                Scene.The.Cursor.ChangeCursorImage(Scene.The.Cursor.VerticalResizeImage);
                break;
        }

        if (InputUtility.WasMouseLeftPressed && IsTouched && 
            (resize_grab_type != ResizeGrabType.None || Handle.IsPointedAt()))
        {
            is_grabbed = true;

            grab_offset = PixelPosition - Scene.The.Cursor.PixelPointedAt;
        }
        if (is_grabbed && InputUtility.WasMouseLeftReleased)
            is_grabbed = false;

        if (is_grabbed)
        {
            if (resize_grab_type == ResizeGrabType.None)
            {
                Vector2Int new_position = Scene.The.Cursor.PixelPointedAt + grab_offset;
                if (new_position != PixelPosition)
                    IsMaximized = false;

                natural_position = new_position;
            }
            else
            {
                Vector2Int new_position = natural_position, 
                           new_size = natural_size;

                int top_edge_height = natural_position.y + natural_size.y;

                Vector2Int displaced_position = Scene.The.Cursor.PixelPointedAt;
                Vector2Int displacement = displaced_position - natural_position;

                if ((resize_grab_type == ResizeGrabType.Left || 
                    resize_grab_type == ResizeGrabType.BottomLeft) && 
                    displaced_position.x <= (natural_position.x + natural_size.x - MinimumSize.x))
                {
                    new_position.x = displaced_position.x;
                    new_size.x += -displacement.x;
                }
                else if(resize_grab_type == ResizeGrabType.Right ||
                        resize_grab_type == ResizeGrabType.BottomRight)
                    new_size.x = displaced_position.x - natural_position.x + 1;

                if(resize_grab_type == ResizeGrabType.BottomLeft ||
                        resize_grab_type == ResizeGrabType.Bottom || 
                        resize_grab_type == ResizeGrabType.BottomRight)
                    new_size.y = (natural_position.y + natural_size.y) - displaced_position.y + 1;

                natural_position = new_position;
                natural_size = new_size;

                natural_size.x = Mathf.Max(natural_size.x, MinimumSize.x);
                natural_size.y = Mathf.Max(natural_size.y, MinimumSize.y);

                natural_position.y += top_edge_height - (natural_position.y + natural_size.y);
            }
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

        if (IsMinimized)
            CanvasGroup.alpha = 0;
    }
}
