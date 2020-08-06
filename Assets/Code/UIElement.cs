using UnityEngine;
using System.Collections;

public class UIElement : MonoBehaviour
{
    public RectTransform RectTransform
    { get { return transform as RectTransform; } }

    public Vector2Int PixelPosition { get { return RectTransform.anchoredPosition.Round(); } }

    public bool IsPointedAt { get { return this.IsPointedAt(); } }
    public bool IsTouched { get { return this.IsTouched(); } }
}
