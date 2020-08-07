using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaximizeWindowButton : WindowButton
{
    protected override void OnButtonUp()
    {
        Window.IsMaximized = !Window.IsMaximized;
    }
}
