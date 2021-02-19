using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SchedulePanelAddStopButton : Button.Script, TransportCraftPanel.Element
{
    public TMPro.TextMeshProUGUI Text;

    public Color ActiveColor,
                 ActiveTouchColor,
                 ActiveDownColor;

    void Update()
    {
        if (this.TransportCraftPanel().SchedulePanel.IsUserChoosingDestination)
        {
            Text.text = "Select Dest.";
            Text.color = Color.black;

            if (IsDown)
                Button.Image.color = ActiveDownColor;
            else if (IsTouched)
                Button.Image.color = ActiveTouchColor;
            else
                Button.Image.color = ActiveColor;
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
