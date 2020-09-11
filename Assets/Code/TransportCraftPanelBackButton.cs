using UnityEngine;
using System.Collections;

public class TransportCraftPanelBackButton : Button, TransportCraftPanel.Element
{
    public CanvasGroup CanvasGroup;

    protected override void Update()
    {
        base.Update();

        CanvasGroup.alpha = 
            this.TransportCraftPanel().State != TransportCraftPanel.MenuState.Main || 
            this.TransportCraftPanel().IsPointedAt ? 1 : 0;
    }

    protected override void OnButtonUp()
    {
        this.TransportCraftPanel().State = this.TransportCraftPanel().State - 1;
    }
}
