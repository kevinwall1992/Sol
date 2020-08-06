using UnityEngine;
using System.Collections;

public class CloseWindowButton : Button
{
    public Window Window;

    protected override void OnButtonUp()
    {
        Window.IsOpen = false;
    }
}
