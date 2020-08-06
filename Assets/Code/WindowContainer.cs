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

    public void AddWindow(Window window)
    {
        window.transform.SetParent(transform);
    }

    public Window RemoveWindow(Window window)
    {
        if (window.transform.parent == transform)
            window.transform.SetParent(null);

        return window;
    }
}
