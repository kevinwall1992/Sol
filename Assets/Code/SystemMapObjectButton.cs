using UnityEngine;
using System.Collections;

public class SystemMapObjectButton : Button
{
    public SystemMapObject SystemMapObject;

    protected override void OnButtonUp()
    {
        Scene.The.SystemMap.FocusedObject = SystemMapObject;
    }
}
