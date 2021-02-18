using UnityEngine;
using System.Collections;

public class ClickableTMP_InputField : UIElement
{
    public TMPro.TMP_InputField InputField;

    private void Update()
    {
        if (WasClicked)
            InputField.Select();
    }
}
