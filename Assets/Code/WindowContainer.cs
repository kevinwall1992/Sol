using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WindowContainer : UIElement
{
    public IEnumerable<Window> Windows
    { get { return GetComponentsInChildren<Window>(); } }

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
        MoveToFront(window);

        return window;
    }

    public Window RemoveWindow(Window window)
    {
        if (window.transform.parent == transform)
            window.transform.SetParent(null);

        return window;
    }

    public void MoveToFront(Window window)
    {
        window.transform.SetAsLastSibling();
    }
}
