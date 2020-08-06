using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scene : MonoBehaviour
{
    public Camera UICamera;
    public Canvas Canvas;
    public GraphicRaycaster GraphicRaycaster;
    public Style Style;
    public Prefabs Prefabs;
    public ShortcutContainer ShortcutContainer;
    public Taskbar Taskbar;
    public WindowContainer WindowContainer;
    public Cursor Cursor;


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
