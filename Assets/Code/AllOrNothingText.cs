using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TMPro.TextMeshProUGUI))]
public class AllOrNothingText : UIElement
{
    Color rest_color;

    public int CharacterWidth = 6;
    public int Margin = 1;

    public TMPro.TextMeshProUGUI Text
    { get { return GetComponent<TMPro.TextMeshProUGUI>(); } }

    private void Start()
    {
        rest_color = Text.color;
    }

    private void Update()
    {
        if ((Text.text.Length * CharacterWidth + Margin) > 
            RectTransform.rect.width)
            Text.color = Color.clear;
        else
            Text.color = rest_color;
    }
}
