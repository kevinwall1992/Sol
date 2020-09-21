using UnityEngine;
using System.Collections;

public class MoveToFrontWhenPressed : UIElement
{
    private void Update()
    {
        if (IsTouched && InputUtility.WasMouseLeftPressed)
            transform.SetAsLastSibling();
    }
}
