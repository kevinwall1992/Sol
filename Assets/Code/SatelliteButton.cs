using UnityEngine;
using System.Collections;

public class SatelliteButton : Button
{
    public Satellite Satellite;

    protected override void OnButtonUp()
    {
        bool is_already_selected_station =
            Satellite.IsStation() &&
            ReferenceEquals(Scene.The.SystemMap.PlacePanel.Place, Satellite.Station());

        if (Satellite.IsVisitable())
        {
            if (Satellite.IsStation() || 
                Satellite.SystemMapObject().IsFocused)
            {
                Scene.The.SystemMap.PlacePanel.Place = Satellite.Place();
                Scene.The.SystemMap.PlacePanel.State = PlacePanel.MenuState.Main;
            }
        }
        else if(Satellite.IsCraft())
        {
            Scene.The.SystemMap.TransportCraftPanel.Craft = Satellite.Craft();
            Scene.The.SystemMap.TransportCraftPanel.State = TransportCraftPanel.MenuState.Main;
        }

        if (!Satellite.IsCraft() || is_already_selected_station)
            Scene.The.SystemMap.FocusedObject = Satellite.SystemMapObject();
    }
}
