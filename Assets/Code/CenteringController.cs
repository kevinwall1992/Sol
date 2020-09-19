using UnityEngine;
using System.Collections;

[ExecuteAlways]
public class CenteringController : UIElement
{
    public RectTransform Centeree;

    private void Update()
    {
        Vector2 center = new Vector2(RectTransform.rect.width / 2 + RectTransform.rect.min.x, RectTransform.rect.height / 2 + RectTransform.rect.min.y);

        Centeree.localPosition = new Vector2(center.x - Centeree.rect.width / 2, center.y - Centeree.rect.height / 2);
    }
}
