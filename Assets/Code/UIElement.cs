using UnityEngine;
using System.Collections;

public class UIElement : MonoBehaviour
{
    public RectTransform RectTransform
    { get { return transform as RectTransform; } }

    public Vector2Int PixelPosition { get { return RectTransform.anchoredPosition.Round(); } }

    public virtual bool IsPointedAt { get { return this.IsPointedAt(); } }
    public virtual bool IsTouched { get { return this.IsTouched(); } }
}
