using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SchedulePanelAddStopButton : Button, TransportCraftPanel.Element
{
    public TMPro.TextMeshProUGUI Text;

    public Color ActiveColor,
                 ActiveTouchColor,
                 ActiveDownColor;

    protected override void Update()
    {
        base.Update();

        if (this.TransportCraftPanel().SchedulePanel.IsUserChoosingDestination)
        {
            Text.text = "Select Dest.";
            Text.color = Color.black;

            if (IsDown)
                Image.color = ActiveDownColor;
            else if (IsTouched)
                Image.color = ActiveTouchColor;
            else
                Image.color = ActiveColor;
        }
        else
        {
            Text.text = "Travel To...";
            Text.color = Color.white;
        }
    }

    protected override void OnButtonUp()
    {
        this.TransportCraftPanel().SchedulePanel.IsUserChoosingDestination = 
            !this.TransportCraftPanel().SchedulePanel.IsUserChoosingDestination;
    }
}
