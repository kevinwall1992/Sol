using UnityEngine;

public class ScheduleElement : UIElement, TransportCraftPanel.Element
{
    public Navigation.Transfer DepartureTransfer;

    public TMPro.TextMeshProUGUI PlaceText, 
                                 StayText,
                                 StayDurationText,
                                 TravelDurationText,
                                 InTransitText;

    public RectTransform TravelDivider;

    public SchedulePanelRemoveStopButton RemoveTransferButton;

    public Navigation.Transfer ArrivalTransfer
    {
        get
        {
            Navigation navigation = this.TransportCraftPanel().Craft.Navigation;

            int arrival_transfer_index;
            if (DepartureTransfer == null)
                arrival_transfer_index = navigation.Transfers.Count - 1;
            else
                arrival_transfer_index = navigation.Transfers.IndexOf(DepartureTransfer) - 1;

            if (arrival_transfer_index < 0)
                return null;

            return navigation.Transfers[arrival_transfer_index];
        }
    }

    public System.DateTime ArrivalDate
    {
        get
        {
            System.DateTime arrival_date = Scene.The.Clock.Now;

            if (ArrivalTransfer != null && ArrivalTransfer.ArrivalDate > arrival_date)
                arrival_date = ArrivalTransfer.ArrivalDate;

            return arrival_date;
        }
    }

    public System.DateTime DepartureDate
    {
        get
        {
            if (IsInTransit)
                return Scene.The.Clock.Now;

            return DepartureTransfer.DepartureDate;
        }
    }

    public Visitable Place
    {
        get
        {
            SystemMapObject primary;

            if (IsInTransit || ArrivalTransfer == null)
                primary = this.TransportCraftPanel().Craft.Primary;
            else
                primary = ArrivalTransfer.TargetMotion.Primary;

            if (!primary.IsVisitable())
                return null;

            return primary.Place();
        }
    }

    public SatelliteMotion Motion
    {
        get
        {
            if (DepartureTransfer != null)
                return DepartureTransfer.OriginalMotion;
            else if (ArrivalTransfer != null)
                return ArrivalTransfer.TargetMotion;
            else
                return this.TransportCraftPanel().Craft.Motion;
        }
    }

    public bool IsInTransit
    {
        get
        {
            return DepartureTransfer != null && 
                   DepartureTransfer.DepartureDate < Scene.The.Clock.Now;
        }
    }

    private void Update()
    {
        RemoveTransferButton.Transfer = DepartureTransfer;

        if (!IsInTransit)
        {
            InTransitText.gameObject.SetActive(false);

            PlaceText.text = Place.PlaceName;

            StayText.text = InShortDateFormat(ArrivalDate) + "-";
            if (DepartureTransfer != null)
            {
                StayText.text += InShortDateFormat(DepartureDate);

                StayDurationText.text =
                    TimeSpanToDurationString(DepartureDate - ArrivalDate);
            }
            else
                StayDurationText.text = "";
        }
        else
        {
            InTransitText.gameObject.SetActive(true);

            PlaceText.text = "";
            StayText.text = "";
            StayDurationText.text = "";
        }

        if(DepartureTransfer != null)
        {
            TravelDivider.gameObject.SetActive(true);

            TravelDurationText.text = 
                "Travel " + 
                TimeSpanToDurationString(DepartureTransfer.ArrivalDate - DepartureDate);
        }
        else
            TravelDivider.gameObject.SetActive(false);
    }

    public static string TimeSpanToDurationString(System.TimeSpan time_span)
    {
        if (time_span.TotalDays / (365.0f / 12) >= 100)
            return ((int)(time_span.TotalDays / 365)).ToString("D2") + " Yrs";
        if (time_span.TotalDays >= 100)
            return ((int)(time_span.TotalDays / (365.0f / 12))).ToString("D2") + " Mo";
        else if(time_span.TotalHours >= 100)
            return ((int)time_span.TotalDays).ToString("D2") + " Days";
        else if(time_span.TotalMinutes >= 100)
            return ((int)time_span.TotalHours).ToString("D2") + " Hrs";
        else if(time_span.TotalSeconds >= 100)
            return ((int)time_span.TotalMinutes).ToString("D2") + " Mins";
        else
            return ((int)time_span.TotalSeconds).ToString("D2") + " Secs";
    }

    public static string GetMonthAbbreviation(int month_index)
    {
        switch (month_index)
        {
            case 0: return "Jan";
            case 1: return "Feb";
            case 2: return "Mar";
            case 3: return "Apr";
            case 4: return "May";
            case 5: return "Jun";
            case 6: return "Jul";
            case 7: return "Aug";
            case 8: return "Sep";
            case 9: return "Oct";
            case 10: return "Nov";
            case 11: return "Dec";

            default: return "";
        }
    }

    public static string InShortDateFormat(System.DateTime date)
    {
        return GetMonthAbbreviation(date.Month - 1) + 
               date.Day.ToString("D2");
    }

    public static string InLongDateFormat(System.DateTime date)
    {
        return GetMonthAbbreviation(date.Month - 1) + " " + 
               date.Day.ToString("D2") + ", " + 
               date.Year;
    }
}
