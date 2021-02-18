using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ItemCollectionPanelLine : ItemCollectionPanel.Element
{
    public Image Background;

    public TMPro.TextMeshProUGUI DescriptionText, QuantityText;

    public Color BackgroundColor,
                 BackgroundHighlightColor;

    public int ShortNameWidth = 150;

    protected override void Update()
    {
        base.Update();

        if (RectTransform.rect.width < ShortNameWidth)
            DescriptionText.text = Item.ShortName;
        else
            DescriptionText.text = 
                Item.Name + 
                (Item.Qualifier == "" ? "" : " - " + Item.Qualifier);

        QuantityText.text = Item.GetQuantityString();

        Background.color = IsPointedAt ? BackgroundHighlightColor : 
                                         BackgroundColor;
    }
}
