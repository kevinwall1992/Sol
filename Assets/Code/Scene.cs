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
    public string SessionUsername;


    public Bank DefaultBank;

    public IEnumerable<Craft> Crafts
    { get { return FindObjectsOfType<Craft>(); } }

    public IEnumerable<Station> Stations
    { get { return Crafts.SelectComponents<Craft, Station>(); } }

    public IEnumerable<User> Users
    { get { return FindObjectsOfType<User>(); } }

    public User SessionUser
    {
        get
        {
            return Users.FirstOrDefault(
                user => user.Username == SessionUsername);
        }
    }

    public IEnumerable<Bank> Banks
    { get { return Users.SelectComponents<User, Bank>(); } }


    private void Start()
    {
        
    }


    static Scene the;
    public static Scene The
    {
        get
        {
            if (the == null)
                the = GameObject.FindObjectOfType<Scene>();

            return the;
        }
    }
}
