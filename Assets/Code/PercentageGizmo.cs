using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(AnimatorController))]
public class PercentageGizmo : UIElement
{
    public float Value;

    public bool IsBeingPulled { get; private set; }

    public Image Background, AnimatedImage, MaximumValueOverlay;

    public TMPro.TextMeshProUGUI Text;

    public Color TouchedColor;

    public AnimatorController AnimatorController
    { get { return GetComponent<AnimatorController>(); } }

    private void Update()
    {
        Background.color = IsTouched ? TouchedColor : Color.white;

        if (IsTouched && InputUtility.WasMouseLeftPressed)
            IsBeingPulled = true;

        if (InputUtility.WasMouseLeftReleased)
            IsBeingPulled = false;


        if(IsBeingPulled && InputUtility.IsMouseLeftPressed)
        {
            Value += (9 * Value + 0.5f) / 10 * 
                     Input.GetAxis("Mouse Y") * 0.1f;
        }
        Value = Mathf.Clamp(Value, 0, 1);


        AnimatorController.Moment = Value;

        if (Value < 1)
        {
            Text.text = (Value * 100).RoundDown().ToString("00");
            MaximumValueOverlay.gameObject.SetActive(false);
        }
        else
        {
            Text.text = "";
            MaximumValueOverlay.gameObject.SetActive(true);
        }
    }
}
