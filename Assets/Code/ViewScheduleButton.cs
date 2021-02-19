using UnityEngine;
using System.Collections;

public class ViewScheduleButton : Button.Script, TransportCraftPanel.Element
{
    protected override void OnButtonUp()
    {
        this.TransportCraftPanel().State = 
            TransportCraftPanel.MenuState.Schedule;
    }
}
