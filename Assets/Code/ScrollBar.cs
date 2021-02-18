using UnityEngine;
using System.Collections;

[ExecuteAlways]
public class ScrollBar : UIElement
{
    public RectTransform Opening, Drawer, Bar;

    public CanvasGroup CanvasGroup;

    public float BaseDisplacementToBarLengthRatio = 1;
    public float MinimumBarLength = 10;

    public bool IsVertical = true;

    public float DisplacementToBarLengthRatio
    {
        get
        {
            if (MaxDisplacement == 0)
                return BaseDisplacementToBarLengthRatio;

            return MaxDisplacement / 
                  ((IsVertical ? RectTransform.rect.height : 
                                 RectTransform.rect.width) - BarLength);
        }
    }

    public float Displacement
    {
        get
        {
            return IsVertical ? -Bar.localPosition.y * DisplacementToBarLengthRatio : 
                                Bar.localPosition.x * DisplacementToBarLengthRatio;
        }

        set
        {
            Bar.localPosition = 
                IsVertical ? -Bar.localPosition.YChangedTo(value / DisplacementToBarLengthRatio) :
                             Bar.localPosition.XChangedTo(value / DisplacementToBarLengthRatio);
        }
    }

    public float MaxDisplacement
    {
        get
        {
            return IsVertical ? Mathf.Max(0, Drawer.rect.height - Opening.rect.height) : 
                                Mathf.Max(0, Drawer.rect.width - Opening.rect.width);
        }
    }

    public float Progress
    {
        get { return Displacement / MaxDisplacement; }
        set { Displacement = value * MaxDisplacement; }
    }

    public float BarLength
    {
        get
        {
            return Mathf.Max(
                MinimumBarLength, 
                RectTransform.rect.height -
                MaxDisplacement / BaseDisplacementToBarLengthRatio);
        }
    }

    private void Start()
    {
        Progress = 0;
    }

    private void Update()
    {
        if(MaxDisplacement == 0)
        {
            CanvasGroup.alpha = 0;
            CanvasGroup.blocksRaycasts = false;
            return;
        }
        else
        {
            CanvasGroup.alpha = 1;
            CanvasGroup.blocksRaycasts = true;
        }

        Bar.sizeDelta = 
            IsVertical ? Bar.sizeDelta.YChangedTo(BarLength) : 
                         Bar.sizeDelta.XChangedTo(BarLength);

        Displacement = Mathf.Max(0, Mathf.Min(MaxDisplacement, Displacement));

        Bar.localPosition = Bar.localPosition.XChangedTo(0);

        Drawer.localPosition = 
            IsVertical ? Drawer.localPosition.YChangedTo(Displacement) :
                         Drawer.localPosition.XChangedTo(-Displacement);
    }
}
