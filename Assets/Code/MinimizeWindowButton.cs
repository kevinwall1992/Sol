using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimizeWindowButton : WindowButton
{
    protected override void OnButtonUp()
    {
        Window.IsMinimized = true;
    }
}
