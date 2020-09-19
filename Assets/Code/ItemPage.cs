using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ItemPage : Page
{
    public Image Image;

    public TMPro.TextMeshProUGUI NameText, 
                                 QualifierText, 
                                 DescriptionText;

    public Item Item;

    private void Update()
    {
        Image.sprite = Item.ProfilePicture;

        NameText.text = Item.Name;
        QualifierText.text = Item.Qualifier;
        DescriptionText.text = Item.Description;
    }


    public static ItemPage Create(Item item)
    {
        ItemPage item_page = GameObject.Instantiate(Scene.The.Prefabs.ItemPage);
        item_page.Item = item;

        return item_page;
    }
}
