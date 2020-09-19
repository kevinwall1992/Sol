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

        Window.Create().Open(ItemPage.Create(item));
    }
}
