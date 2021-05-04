using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Scene : MonoBehaviour
{
    public Clock Clock;
    public Camera UICamera;
    public Canvas Canvas;
    public GraphicRaycaster GraphicRaycaster;
    public TravelingElementHotel _3DUIElementsContainer;
    public Style Style;
    public Prefabs Prefabs;
    public ShortcutContainer ShortcutContainer;
    public Taskbar Taskbar;
    public WindowContainer WindowContainer;
    public Cursor Cursor;
    public SystemMap SystemMap;
    public CraftInventoryPage CraftInventoryPage;
    public ItemPage ItemPage;
    public RefuelingPage RefuelingPage;
    public StationViewer StationViewer;
    public ItemDatabase ItemDatabase;

    public Bank DefaultBank;
    public string SessionUsername;

}
