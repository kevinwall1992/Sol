using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CanvasController : MonoBehaviour
{
    public Canvas Canvas;

    void Update()
    {
        RectTransform canvas_rect_transform = Canvas.transform as RectTransform;

        canvas_rect_transform.sizeDelta = canvas_rect_transform.sizeDelta.Round();
        canvas_rect_transform.localScale = 8 * Vector2.one / canvas_rect_transform.sizeDelta.y;
    }
}
