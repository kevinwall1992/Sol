using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PixelSnapper : MonoBehaviour
{
    Vector2 remainder;

    void Update()
    {
        RectTransform rect_transform = transform as RectTransform;

        Vector2 position = rect_transform.anchoredPosition;
        if (UnityEditor.EditorApplication.isPlaying)
            position = remainder + rect_transform.anchoredPosition;

        rect_transform.anchoredPosition = new Vector2(MathUtility.Round(position.x), MathUtility.Round(position.y));
        remainder = position - rect_transform.anchoredPosition;
    }
}
