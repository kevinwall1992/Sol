using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class PixelSnapper : MonoBehaviour
{
    void Update()
    {
        RectTransform rect_transform = transform as RectTransform;

        rect_transform.anchoredPosition = rect_transform.anchoredPosition.Round();

        if(Application.isEditor && !Application.isPlaying)
            rect_transform.sizeDelta = rect_transform.sizeDelta.Round();
    }
}
