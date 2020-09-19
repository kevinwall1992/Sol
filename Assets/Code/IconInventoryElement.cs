using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class IconInventoryElement : UIElement
{
    Color name_label_rest_color;

    public Image Icon;

    public Label NameLabel, QuantityLabel, QualifierLabel;

    public Color LabelHighlightColor;

    [HideInInspector]
    public Item Item;

    private void Start()
    {
        name_label_rest_color = NameLabel.BackgroundColor;
    }

    private void Update()
    {
        if (Item == null)
            return;

        NameLabel.Text.text = IsPointedAt ? Item.Name : Item.ShortName;
        NameLabel.BackgroundColor = IsPointedAt ? LabelHighlightColor : 
                                                  name_label_rest_color;

        QuantityLabel.Text.text = Item.GetQuantityString();

        QualifierLabel.gameObject.SetActive(IsPointedAt);
        QualifierLabel.Text.text = Item.Qualifier;

        Icon.sprite = Item.Icon;
    }
}
