using UnityEngine;
using System.Collections;

public class MoveToFrontWhenClicked : UIElement
{
    private void Update()
    {
        if (IsTouched && InputUtility.WasMouseLeftReleased)
            transform.SetAsLastSibling();
    }
}
