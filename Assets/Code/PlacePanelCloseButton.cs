using UnityEngine;
using System.Collections;

public class PlacePanelCloseButton : Button.Script, PlacePanel.Element
{
    public CanvasGroup CanvasGroup;

    protected void Update()
    {
        CanvasGroup.alpha = this.PlacePanel().IsPointedAt ? 1 : 0;
    }

    protected override void OnButtonUp()
    {
        this.PlacePanel().State = PlacePanel.MenuState.Hidden;
    }
}
