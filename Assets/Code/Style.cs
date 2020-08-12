using UnityEngine;
using System.Collections;

public class Style : MonoBehaviour
{
    public Vector2Int MonitorResolution
    { get { return (Scene.The.Canvas.transform as RectTransform).rect.size.Round(); } }

    public int StandardEdgeThickness = 2;

    public int TaskbarElementWidth = 100;
}
