using UnityEngine;
using System.Collections;

public class Page : UIElement
{
    public Window Window { get { return GetComponentInParent<Window>(); } }
}
