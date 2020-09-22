using UnityEngine;
using System.Collections;

public class ViewRefuelingPageButton : Button, TransportCraftPanel.Element
{
    protected override void OnButtonUp()
    {
        Scene.The.RefuelingPage.Craft = this.TransportCraftPanel().Craft;
        Scene.The.RefuelingPage.Window.Open();
    }
}
