using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LineInventoryElement : UIElement
{
    public Image Background;

    public TMPro.TextMeshProUGUI DescriptionText, QuantityText;

    public Color BackgroundColor,
                 BackgroundHighlightColor;

    [HideInInspector]
    public Item Item;

    private void Update()
    {
        DescriptionText.text = Item.Name + " - " + Item.Qualifier;
        QuantityText.text = Item.GetQuantityString();

        Background.color = IsPointedAt ? BackgroundHighlightColor : 
                                         BackgroundColor;
    }
}
