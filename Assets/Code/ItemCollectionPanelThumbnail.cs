using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class ItemCollectionPanelThumbnail : ItemCollectionPanel.Element
{
    Color name_label_rest_color;

    public Image Icon;

    public Label NameLabel, QuantityLabel, QualifierLabel;

    public Color LabelHighlightColor;

    private void Start()
    {
        name_label_rest_color = NameLabel.BackgroundColor;
    }

    protected override void Update()
    {
        base.Update();

        if (Item == null)
            return;

        NameLabel.Text.text = IsPointedAt ? Item.Name : Item.ShortName;
        NameLabel.BackgroundColor = IsPointedAt ? LabelHighlightColor : 
                                                  name_label_rest_color;

        QuantityLabel.Text.text = Item.GetQuantityString();

        QualifierLabel.Text.text = Item.Qualifier;

        Icon.sprite = Item.Icon;
    }
}
