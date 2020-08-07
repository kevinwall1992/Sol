using UnityEngine;
using System.Collections;

public class CloseWindowButton : WindowButton
{
    protected override void OnButtonUp()
    {
        Window.IsOpen = false;
    }
}
