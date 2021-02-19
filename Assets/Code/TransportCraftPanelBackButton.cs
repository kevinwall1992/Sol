using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class TransportCraftPanelBackButton : Button.Script, 
                                             TransportCraftPanel.Element
{
    public CanvasGroup CanvasGroup
    { get { return GetComponent<CanvasGroup>(); } }

    void Update()
    {
        CanvasGroup.alpha = 
            this.TransportCraftPanel().State != TransportCraftPanel.MenuState.Main || 
            this.TransportCraftPanel().IsPointedAt ? 1 : 0;
    }

    protected override void OnButtonUp()
    {
        this.TransportCraftPanel().State = this.TransportCraftPanel().State - 1;
    }
}
