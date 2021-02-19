using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public static class The
{
    //TheShortcuts.py (Don't modify this comment)

    public static Clock Clock { get { return Scene.Clock; } }
    public static Camera UICamera { get { return Scene.UICamera; } }
    public static Canvas Canvas { get { return Scene.Canvas; } }
    public static GraphicRaycaster GraphicRaycaster { get { return Scene.GraphicRaycaster; } }
    public static TravelingElementHotel _3DUIElementsContainer { get { return Scene._3DUIElementsContainer; } }
    public static Style Style { get { return Scene.Style; } }
    public static Prefabs Prefabs { get { return Scene.Prefabs; } }
    public static ShortcutContainer ShortcutContainer { get { return Scene.ShortcutContainer; } }
    public static Taskbar Taskbar { get { return Scene.Taskbar; } }
    public static WindowContainer WindowContainer { get { return Scene.WindowContainer; } }
    public static Cursor Cursor { get { return Scene.Cursor; } }
    public static SystemMap SystemMap { get { return Scene.SystemMap; } }
    public static CraftInventoryPage CraftInventoryPage { get { return Scene.CraftInventoryPage; } }
    public static ItemPage ItemPage { get { return Scene.ItemPage; } }
    public static RefuelingPage RefuelingPage { get { return Scene.RefuelingPage; } }
    public static StationViewer StationViewer { get { return Scene.StationViewer; } }
    public static Bank DefaultBank { get { return Scene.DefaultBank; } }
    public static string SessionUsername { get { return Scene.SessionUsername; } }


    public static IEnumerable<Craft> Crafts
    { get { return GameObject.FindObjectsOfType<Craft>(); } }

    public static IEnumerable<Station> Stations
    { get { return Crafts.SelectComponents<Craft, Station>(); } }

    public static IEnumerable<User> Users
    { get { return GameObject.FindObjectsOfType<User>(); } }

    public static User SessionUser
    {
        get
        {
            return Users.FirstOrDefault(
                user => user.Username == SessionUsername);
        }
    }

    public static IEnumerable<Bank> Banks
    { get { return Users.SelectComponents<User, Bank>(); } }


    static Scene scene;
    public static Scene Scene
    {
        get
        {
            if (scene == null)
                scene = GameObject.FindObjectOfType<Scene>();

            return scene;
        }
    }
}
