using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Cursor : MonoBehaviour
{
    public float Sensitivity = 1;

    void Start()
    {
        UnityEngine.Cursor.visible = false;
    }

    void Update()
    {
        RectTransform rect_transform = transform as RectTransform;

        rect_transform.anchoredPosition +=
            new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) *
            Sensitivity;

        Vector2 screen_resolution = 
            (GetComponentInParent<Canvas>().transform as RectTransform).rect.size;

        rect_transform.anchoredPosition = 
            new Vector2(Mathf.Min(Mathf.Max(rect_transform.anchoredPosition.x, 0), screen_resolution.x), 
                        Mathf.Max(Mathf.Min(rect_transform.anchoredPosition.y, 0), -screen_resolution.y));
    }
}
