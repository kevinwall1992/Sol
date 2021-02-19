using UnityEngine;
using System.Collections;

public class Style : MonoBehaviour
{
    public Vector2Int MonitorResolution
    { get { return (The.Canvas.transform as RectTransform).rect.size.Round(); } }

    public int StandardEdgeThickness = 2;
}
