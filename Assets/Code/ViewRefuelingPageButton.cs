using UnityEngine;
using System.Collections;

public class ViewRefuelingPageButton : Button.Script, TransportCraftPanel.Element
{
    protected override void OnButtonUp()
    {
        The.RefuelingPage.Craft = this.TransportCraftPanel().Craft;
        The.RefuelingPage.Window.Open();
    }
}
