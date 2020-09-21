using UnityEngine;
using System.Collections;

public class InventoryElementButton : Button
{
    protected override void Update()
    {
        base.Update();
    }

    protected override void OnButtonUp()
    {
        Item item = InventoryPage.GetItemFromInventoryElement(gameObject);

        Scene.The.ItemPage.Item = item;
        Scene.The.ItemPage.Window.Open();
    }
}
