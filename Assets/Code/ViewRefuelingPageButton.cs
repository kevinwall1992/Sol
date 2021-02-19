using UnityEngine;
using System.Collections;

public class ViewRefuelingPageButton : Button, TransportCraftPanel.Element
{
    protected override void OnButtonUp()
    {
        The.RefuelingPage.Craft = this.TransportCraftPanel().Craft;
        The.RefuelingPage.Window.Open();
    }
}
