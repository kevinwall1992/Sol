using UnityEngine;
using System.Collections;
using UnityEngine.UI;


[ExecuteAlways]
public class ProgressBar : UIElement
{
    public string Label;
    public string Units;
    public string Format;

    public float Value = 0;
    public float MaximumValue = 1;

    public Color Color = Color.white;

    public RectTransform Bar;
    public Image BarImage;
    public TMPro.TextMeshProUGUI LabelText;

    float Progress
    {
        get { return Value / MaximumValue; }
        set { Value = Progress * MaximumValue; }
    }

    private void Start()
    {
        Bar.sizeDelta = Bar.sizeDelta.XChangedTo(0);
    }

    protected virtual void Update()
    {
        Value = Mathf.Min(Mathf.Max(Value, 0), MaximumValue);

        Bar.sizeDelta = Bar.sizeDelta.XChangedTo(
            Mathf.Lerp(Bar.sizeDelta.x, 
                       RectTransform.rect.width * Progress, 
                       CatchupSpeed * Time.deltaTime));

        BarImage.color = Color;

        if (!IsTouched)
            LabelText.text = Label;
        else
            LabelText.text = Value.ToString(Format) + "/" + 
                             MaximumValue.ToString(Format) + " " + 
                             Units;
    }

    public float CatchupSpeed = 1;
}
