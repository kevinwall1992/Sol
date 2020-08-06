using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

public static class InputUtility
{
    public enum MouseButton { Left, Right, Middle }

    public static bool WasMouseLeftPressed { get { return Input.GetMouseButtonDown((int)MouseButton.Left); } }
    public static bool IsMouseLeftPressed { get { return Input.GetMouseButton((int)MouseButton.Left); } }
    public static bool WasMouseLeftReleased { get { return Input.GetMouseButtonUp((int)MouseButton.Left); } }

    public static bool WasMouseRightPressed { get { return Input.GetMouseButtonDown((int)MouseButton.Right); } }
    public static bool IsMouseRightPressed { get { return Input.GetMouseButton((int)MouseButton.Right); } }
    public static bool WasMouseRightReleased { get { return Input.GetMouseButtonUp((int)MouseButton.Right); } }

    public static Vector2 GetMouseMotion()
    {
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

    public static bool ContainsCursor(this RectTransform rect_transform)
    {
        return rect_transform.Contains(Scene.The.Cursor.PixelPointedAt, Scene.The.UICamera);
    }

    public static bool IsPointedAt(this RectTransform rect_transform)
    {
        return rect_transform.ContainsCursor();
    }

    public static bool IsPointedAt(this GameObject game_object)
    {
        if (game_object.transform is RectTransform)
            return (game_object.transform as RectTransform).IsPointedAt();

        return false;
    }

    public static bool IsPointedAt(this MonoBehaviour mono_behaviour)
    {
        return mono_behaviour.gameObject.IsPointedAt();
    }

    public static GameObject TouchedCanvasElement
    {
        get
        {
            PointerEventData pointer_event_data = new PointerEventData(null);
            pointer_event_data.position = Scene.The.Cursor.PixelPointedAt + 
                                          new Vector2(0.5f, 0.5f);

            List<RaycastResult> raycast_results = new List<RaycastResult>();
            Scene.The.Canvas.GetComponent<GraphicRaycaster>().Raycast(pointer_event_data, raycast_results);

            return raycast_results[1].gameObject;
        }
    }

    public static bool IsTouched(this GameObject game_object)
    {
        if (!game_object.IsPointedAt())
            return false;

        if (TouchedCanvasElement == null)
            return false;

        return TouchedCanvasElement.IsChildOf(game_object);
    }

    public static bool IsTouched(this MonoBehaviour mono_behaviour)
    {
        return mono_behaviour.gameObject.IsTouched();
    }

    public static bool IsTouched(this RectTransform rect_transform)
    {
        return rect_transform.gameObject.IsTouched();
    }


    //This should only be called from OnGUI()
    public static bool ConsumeIsKeyUp(KeyCode key_code)
    {
        if (Event.current.isKey && Event.current.keyCode == key_code && Input.GetKeyUp(key_code))
        {
            Event.current.Use();

            return true;
        }

        return false;
    }
}
