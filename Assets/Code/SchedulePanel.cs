using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class SchedulePanel : MonoBehaviour, TransportCraftPanel.Element
{
    List<Navigation.Transfer> transfers_used = new List<Navigation.Transfer>();
    Navigation.Transfer first_transfer_used;

    public int Margin;

    public RectTransform ElementsContainer;

    public SchedulePanelAddStopButton AddTransferButton;

    public bool IsUserChoosingDestination = false;

    public IEnumerable<ScheduleElement> Stops
    { get { return ElementsContainer.GetComponentsInChildren<ScheduleElement>(); } }

    private void Start()
    {
        Clear();
    }

    private void Update()
    {
        Navigation navigation = this.TransportCraftPanel().Craft.Navigation;

        if (transfers_used.SequenceEqual(navigation.Transfers) &&
            first_transfer_used == navigation.NextTransfer && 
            Stops.Count() > 0)
            return;

        Clear();

        float y_offset = 0;

        for (int i = 0; i <= navigation.Transfers.Count; i++)
        {
            Navigation.Transfer transfer = null;
            if (i < navigation.Transfers.Count)
                transfer = navigation.Transfers[i];

            if (transfer != null && transfer.ArrivalDate < The.Clock.Now)
                continue;

            ScheduleElement element = GameObject.Instantiate(ScheduleElementPrefab);
            element.transform.SetParent(ElementsContainer, false);
            element.RectTransform.anchoredPosition = new Vector3(0, y_offset, 0);

            element.DepartureTransfer = transfer;

            y_offset -= element.RectTransform.rect.height + Margin;
        }

        transfers_used = new List<Navigation.Transfer>(navigation.Transfers);
        first_transfer_used = navigation.NextTransfer;

        AddTransferButton.RectTransform.anchoredPosition = 
            new Vector3(0, y_offset + 8, 0);
    }

    public void AddStop(Visitable destination)
    {
        ScheduleElement last_stop = Stops.Last();
        Craft craft = this.TransportCraftPanel().Craft;

        craft.Navigation.AddTransfer(new InterplanetaryTransfer(
            last_stop.Motion,
            destination.GetVisitingMotion(craft),
            last_stop.ArrivalDate));

        IsUserChoosingDestination = false;
    }

    void Clear()
    {
        foreach (ScheduleElement stop in Stops)
            GameObject.Destroy(stop.gameObject);
    }


    public ScheduleElement ScheduleElementPrefab;
}
