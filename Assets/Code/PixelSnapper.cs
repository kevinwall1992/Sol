using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PixelSnapper : MonoBehaviour
{
    Vector2 position_remainder;
    Vector2 size_remainder;

    void Update()
    {
        RectTransform rect_transform = transform as RectTransform;

        Vector2 position = rect_transform.anchoredPosition;
        if (UnityEditor.EditorApplication.isPlaying)
            position += position_remainder;

        rect_transform.anchoredPosition = new Vector2(MathUtility.Round(position.x), 
                                                      MathUtility.Round(position.y));
        position_remainder = position - rect_transform.anchoredPosition;


        Vector2 size = rect_transform.sizeDelta;
        if (UnityEditor.EditorApplication.isPlaying)
            size += size_remainder;

        rect_transform.sizeDelta = new Vector2(MathUtility.Round(size.x),
                                               MathUtility.Round(size.y));
        size_remainder = size - rect_transform.sizeDelta;
    }
}
