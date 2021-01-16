using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class WindowContainer : UIElement
{
    public IEnumerable<Window> Windows
    { get { return transform.Children().SelectComponents<Window>(); } }

    public IEnumerable<Window> WindowsAndSubwindows
    { get { return GetComponentsInChildren<Window>(); } }

    public IEnumerable<Window> OpenWindows
    { get { return Windows.Where(window => window.IsOpen); } }

    public IEnumerable<Window> VisibleWindows
    { get { return OpenWindows.Where(window => !window.IsMinimized); } }

    public bool IsAnyWindowGrabbed
    {
        get
        {
            return WindowsAndSubwindows
                .Where(window => window.IsGrabbed).Count() > 0;
        }
    }

    void Start()
    {

    }

    void Update()
    {

    }

    public Window AddWindow(Window window = null)
    {
        if (window == null)
            window = Window.Create();

        window.transform.SetParent(transform, false);
        window.MoveToFront();

        return window;
    }

    public Window RemoveWindow(Window window)
    {
        if (window.transform.parent == transform)
            window.transform.SetParent(null);

        return window;
    }
}
