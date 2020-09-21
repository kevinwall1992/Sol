using UnityEngine;
using System.Collections;

public class MoveToFrontWhenTouched : UIElement
{
    private void Update()
    {
        if (IsPointedAt)
            transform.SetAsLastSibling();
    }
}
