﻿using UnityEngine;
using System.Collections;

public class Dragger : UIElement
{
    Vector2 grab_offset;

    public bool IsBeingDragged { get; private set; }

    private void Update()
    {
        if (!IsBeingDragged &&
            IsTouched &&
            InputUtility.WasMouseLeftReleased)
        {
            IsBeingDragged = true;
            grab_offset = Scene.The.Canvas.transform.InverseTransformPoint(transform.position) - Scene.The.Cursor.transform.position;
        }

        if (IsBeingDragged)
            transform.position = Scene.The.Cursor.PixelPointedAt + grab_offset;
    }
}
