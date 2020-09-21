using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

[ExecuteAlways]
public class Window : UIElement
{
    Vector2Int natural_size = new Vector2Int(240, 120);
    Vector2Int natural_position = new Vector2Int(50, 50);

    bool is_grabbed = false;
    Vector2Int grab_offset;

    enum ResizeGrabType { None, Left, BottomLeft, Bottom, BottomRight, Right }
    ResizeGrabType resize_grab_type = ResizeGrabType.None;


    public CanvasGroup CanvasGroup;
    public Image BackgroundImage;

    public RectTransform PageContainer;

    public RectTransform TitleBar, 
                         LeftEdge, 
                         BottomLeftCorner, 
                         BottomEdge, 
                         BottomRightCorner, 
                         RightEdge;

    public bool IsOpen;
    public bool IsMinimized;
    public bool IsMaximized;

    public Vector2Int MinimumSize;

    public List<Page> Pages
    { get { return PageContainer.GetComponentsInChildren<Page>().ToList().Reversed(); } }

    public Page FrontPage
    {
        get
        {
            if (Pages.Count == 0)
                return null;

            return Pages.Last();
        }
    }

    public WindowContainer Windows
    { get { return Scene.The.WindowContainer; } }

    private void Start()
    {
        if (!IsOpen)
            gameObject.SetActive(false);
    }

    void Update()
    {
        TitleBar.sizeDelta = TitleBar.sizeDelta.YChangedTo(DefaultTitleBarHeight);
        BackgroundImage.color = DefaultBackgroundColor;

        if (!UnityEditor.EditorApplication.isPlaying)
            return;

        if (Windows == null)
            Scene.The.WindowContainer.AddWindow(this);

        if (!IsOpen)
        {
            gameObject.SetActive(false);
            return;
        }

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
            (resize_grab_type != ResizeGrabType.None || TitleBar.IsPointedAt()))
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
        else
            CanvasGroup.alpha = 1;
    }

    public void Open(Page page = null, bool dont_change_page_order = false)
    {
        IsOpen = true;
        gameObject.SetActive(true);

        Windows.MoveToFront(this);

        if (page == null)
            return;
        
        page.transform.SetParent(PageContainer, false);
        page.gameObject.SetActive(true);
        if(!dont_change_page_order)
            page.transform.SetAsLastSibling();

        foreach (Page other in Pages)
            if (other != page)
                other.gameObject.SetActive(false);
    }

    public void OpenPreviousPage()
    {
        int front_page_index = Pages.IndexOf(FrontPage);

        if (front_page_index > 0)
            Open(Pages[front_page_index - 1], true);
    }

    public void OpenNextPage()
    {
        int front_page_index = Pages.IndexOf(FrontPage);

        if (front_page_index < Pages.Count - 1)
            Open(Pages[front_page_index + 1], true);
    }

    public Page RemovePage(Page page)
    {
        if (page == FrontPage)
            OpenPreviousPage();

        page.transform.SetParent(null);
        Pages.First().gameObject.SetActive(true);

        return page;
    }

    public void Clear()
    {
        foreach(Page page in Pages)
            GameObject.Destroy(page.gameObject);
    }


    public Color DefaultBackgroundColor = Color.black;
    public int DefaultTitleBarHeight = 16;

    public static Window Create()
    {
        Window window = GameObject.Instantiate(Scene.The.Prefabs.Window);
        Scene.The.WindowContainer.AddWindow(window);

        return window;
    }
}
