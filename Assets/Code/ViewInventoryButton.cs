using UnityEngine;
using System.Collections;

public class ViewInventoryButton : Button, TransportCraftPanel.Element
{
    protected override void OnButtonUp()
    {
        Scene.The.CraftInventoryPage.Craft = this.TransportCraftPanel().Craft;
        Scene.The.CraftInventoryPage.InventoryPage.Window.Open();
    }
}
