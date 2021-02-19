using UnityEngine;
using System.Collections;

public class ViewInventoryButton : Button, TransportCraftPanel.Element
{
    protected override void OnButtonUp()
    {
        The.CraftInventoryPage.Craft = this.TransportCraftPanel().Craft;
        The.CraftInventoryPage.InventoryPage.Window.Open();
    }
}
