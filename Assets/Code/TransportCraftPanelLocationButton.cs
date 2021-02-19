using UnityEngine;
using System.Collections;

public class TransportCraftPanelLocationButton : Button.Script, 
                                                 TransportCraftPanel.Element
{
    public TMPro.TextMeshProUGUI LocationText;

    void Update()
    {
        LocationText.text = this.TransportCraftPanel().Craft.Primary.Name;
    }

    protected override void OnButtonUp()
    {
        throw new System.NotImplementedException();
    }
}
