using UnityEngine;
using System.Collections;
using System.Linq;

public class SchedulePanelRemoveStopButton : Button.Script, TransportCraftPanel.Element
{
    public Navigation.Transfer Transfer;

    public CanvasGroup CanvasGroup;

    public ScheduleElement ScheduleElement
    { get { return GetComponentInParent<ScheduleElement>(); } }

    void Update()
    {
        Navigation navigation = this.TransportCraftPanel().Craft.Navigation;

        if (!ScheduleElement.IsTouched || 
            navigation.Transfers.Count == 0 ||
            navigation.Transfers.Last() != Transfer)
        {
            CanvasGroup.alpha = 0;
            CanvasGroup.blocksRaycasts = false;
        }
        else
        {
            CanvasGroup.alpha = 1;
            CanvasGroup.blocksRaycasts = true;
        }
    }

    protected override void OnButtonUp()
    {
        this.TransportCraftPanel().Craft.Navigation.RemoveTransfer(Transfer);
    }
}
