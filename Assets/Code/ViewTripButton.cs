using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewTripButton : Button.Script, TransportCraftPanel.Element
{
    public ScheduleElement ScheduleElement
    { get { return GetComponentInParent<ScheduleElement>(); } }

    protected override void OnButtonUp()
    {
        this.TransportCraftPanel().TripPanel.ScheduleElement = ScheduleElement;

        this.TransportCraftPanel().State = TransportCraftPanel.MenuState.Trip;
    }
}
