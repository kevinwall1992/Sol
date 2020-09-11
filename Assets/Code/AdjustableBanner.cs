using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteAlways]
public class AdjustableBanner : MonoBehaviour
{
    public RectTransformLerper RectTransformLerper;

    public RectTransform ImageContainer
    { get { return RectTransformLerper.Lerpee.parent as RectTransform; } }

    public RectTransform ToolAContainer
    { get { return RectTransformLerper.A.parent as RectTransform; } }

    public RectTransform ToolBContainer
    { get { return RectTransformLerper.B.parent as RectTransform; } }

    public Image Image, ToolImageA, ToolImageB;

    void Update()
    {
        if (RectTransformLerper.Lerpee == null)
            return;

        if (!ImageContainer.HasComponent<RectMask2D>())
            ImageContainer.gameObject.AddComponent<RectMask2D>();

        if (ToolImageA.sprite != Image.sprite)
        {
            ToolImageA.sprite = ToolImageB.sprite = 
                Image.sprite;

            RectTransformLerper.A.sizeDelta = RectTransformLerper.B.sizeDelta =
                RectTransformLerper.Lerpee.sizeDelta;
        }

        RectTransformLerper.LerpFactor = 
            (ImageContainer.rect.width - ToolAContainer.rect.width) /
            (ToolBContainer.rect.width - ToolAContainer.rect.width);
    }
}
