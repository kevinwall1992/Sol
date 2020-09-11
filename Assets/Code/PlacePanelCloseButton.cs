using UnityEngine;
using System.Collections;

public class PlacePanelCloseButton : Button, PlacePanel.Element
{
    public CanvasGroup CanvasGroup;

    protected override void Update()
    {
        base.Update();

        CanvasGroup.alpha = this.PlacePanel().IsPointedAt ? 1 : 0;
    }

    protected override void OnButtonUp()
    {
        this.PlacePanel().State = PlacePanel.MenuState.Hidden;
    }
}
