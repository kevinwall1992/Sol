using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

[ExecuteAlways]
public class ClingWrap : UIElement
{
    public bool VerticalWrap = true, 
                HorizontalWrap = true;

    void Update()
    {
        Vector2 min = Vector2.zero, 
                max = Vector2.zero;

        IEnumerable<RectTransform> children = 
            transform.Children().Select(child => child as RectTransform);
        foreach (RectTransform child in children)
        {
            Vector2 child_min = transform.parent
                .InverseTransformPoint(child.TransformPoint(child.rect.min));
            Vector2 child_max = transform.parent
                .InverseTransformPoint(child.TransformPoint(child.rect.max));

            if (child_min.x > child_max.x)
                Utility.Swap(ref child_min.x, ref child_max.x);
            if (child_min.y > child_max.y)
                Utility.Swap(ref child_min.y, ref child_max.y);

            min = new Vector2(Mathf.Min(min.x, child_min.x), 
                              Mathf.Min(min.y, child_min.y));
            max = new Vector2(Mathf.Max(max.x, child_max.x), 
                              Mathf.Max(max.y, child_max.y));
        }

        if(HorizontalWrap)
            RectTransform.sizeDelta = 
                RectTransform.sizeDelta.XChangedTo(max.x - min.x);

        if (VerticalWrap)
            RectTransform.sizeDelta = 
                RectTransform.sizeDelta.YChangedTo(max.y - min.y);
    }
}
