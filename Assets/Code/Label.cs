using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteAlways]
public class Label : UIElement
{
    public TMPro.TextMeshProUGUI Text;
    public Image Background;
    public Color TextColor, BackgroundColor;

    public int CharacterWidth;

    public bool AutomaticWidth = false;

    private void Update()
    {
        Text.color = TextColor;
        Background.color = BackgroundColor;

        RectTransform background_transform = 
            Background.transform as RectTransform;

        background_transform.sizeDelta =
            background_transform.sizeDelta.XChangedTo(
                Text.text != "" ? Text.renderedWidth.Round() + 1 : 0);

        if (AutomaticWidth)
        {
            RectTransform.sizeDelta = background_transform.sizeDelta;
            Text.overflowMode = TMPro.TextOverflowModes.Overflow;
        }
    }
}
