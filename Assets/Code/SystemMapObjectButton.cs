using UnityEngine;
using System.Collections;

public class SystemMapObjectButton : Button
{
    public SystemMapObject SystemMapObject;

    protected override void OnButtonUp()
    {
        bool is_already_selected_station =
            SystemMapObject.IsStation() &&
            ReferenceEquals(Scene.The.SystemMap.PlacePanel.Place, SystemMapObject.Station());

        if (SystemMapObject.IsVisitable())
        {
            if (SystemMapObject.IsStation() || 
                Scene.The.SystemMap.FocusedObject == SystemMapObject)
            {
                Scene.The.SystemMap.PlacePanel.Place = SystemMapObject.Place();
                Scene.The.SystemMap.PlacePanel.State = PlacePanel.MenuState.Main;
            }
        }
        else if(SystemMapObject.IsCraft())
        {
            Scene.The.SystemMap.TransportCraftPanel.Craft = SystemMapObject.Craft();
            Scene.The.SystemMap.TransportCraftPanel.State = TransportCraftPanel.MenuState.Main;
        }

        if (!SystemMapObject.IsCraft() || is_already_selected_station)
            Scene.The.SystemMap.FocusedObject = SystemMapObject;
    }
}
