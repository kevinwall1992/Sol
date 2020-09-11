using UnityEngine;
using System.Collections;


public class TripPanel : MonoBehaviour, TransportCraftPanel.Element
{
    public ScheduleElement ScheduleElement;

    public TMPro.TextMeshProUGUI 
        OriginText, DestinationText, 
        DepartureDateText, ArrivalDateText;

    public Navigation.Transfer Transfer
    { get { return ScheduleElement.DepartureTransfer; } }

    public SystemMapObject Origin
    { get { return ScheduleElement.DepartureTransfer.OriginalMotion.Primary; } }

    public SystemMapObject Destination
    { get { return ScheduleElement.DepartureTransfer.TargetMotion.Primary; } }

    private void Update()
    {
        if (ScheduleElement == null)
            return;

        OriginText.text = Origin.Name;
        DestinationText.text = Destination.Name;

        DepartureDateText.text = 
            ScheduleElement.InLongDateFormat(Transfer.DepartureDate);

        ArrivalDateText.text = 
            ScheduleElement.InLongDateFormat(Transfer.ArrivalDate);
    }
}
