using UnityEngine;
using System.Collections;

public class TransportCraftPanelLocationButton : Button, TransportCraftPanel.Element
{
    public TMPro.TextMeshProUGUI LocationText;

    protected override void Update()
    {
        base.Update();

        LocationText.text = this.TransportCraftPanel().Craft.Primary.Name;
    }

    protected override void OnButtonUp()
    {
        throw new System.NotImplementedException();
    }
}
