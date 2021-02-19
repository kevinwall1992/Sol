using UnityEngine;
using System.Collections;

public class SatelliteButton : Button.Script
{
    public Satellite Satellite;

    protected override void OnButtonUp()
    {
        bool is_already_selected_station =
            Satellite.IsStation() &&
            ReferenceEquals(The.SystemMap.PlacePanel.Place, Satellite.Station());

        if (Satellite.IsVisitable())
        {
            if (Satellite.IsStation() || 
                Satellite.SystemMapObject().IsFocused)
            {
                The.SystemMap.PlacePanel.Place = Satellite.Place();
                The.SystemMap.PlacePanel.State = PlacePanel.MenuState.Main;
            }
        }
        else if(Satellite.IsCraft())
        {
            The.SystemMap.TransportCraftPanel.Craft = Satellite.Craft();
            The.SystemMap.TransportCraftPanel.State = TransportCraftPanel.MenuState.Main;
        }

        if (!Satellite.IsCraft() || is_already_selected_station)
            The.SystemMap.FocusedObject = Satellite.SystemMapObject();
    }
}
