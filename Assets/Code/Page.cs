using UnityEngine;
using System.Collections;

public class Page : UIElement
{
    public string Title;

    public Window Window
    {
        get
        {
            return transform.parent
                .GetComponentInParent<Window>(true);
        }
    }
}
