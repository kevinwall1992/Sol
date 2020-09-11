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
    
    public static bool IsPixelPositionWithinBounds(this RectTransform rect_transform, 
                                                   Vector2Int pixel_position)
    {
        Vector2 min = Scene.The.Canvas.transform
            .InverseTransformPoint(rect_transform
                .TransformPoint(rect_transform.rect.min));

        Vector2 max = Scene.The.Canvas.transform
            .InverseTransformPoint(rect_transform
                .TransformPoint(rect_transform.rect.max));

        if (pixel_position.x < min.x || pixel_position.x > max.x ||
            pixel_position.y < min.y || pixel_position.y > max.y)
            return false;

        return true;
    }

    public static bool IsCursorWithinBounds(this RectTransform rect_transform)
    {
        return rect_transform.IsPixelPositionWithinBounds(Scene.The.Cursor.PixelPosition);
    }

    public static bool IsPointedAt(this RectTransform rect_transform)
    {
        if (Scene.The.Cursor.CanvasElementsPointedAt == null)
            return false;

        foreach (GameObject game_object in Scene.The.Cursor.CanvasElementsPointedAt)
            if (game_object.IsChildOf(rect_transform))
                return true;

        return false;
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

    public static bool IsTouched(this GameObject game_object)
    {
        if (!game_object.IsPointedAt())
            return false;

        if (Scene.The.Cursor.CanvasElementTouched == null)
            return false;

        return Scene.The.Cursor.CanvasElementTouched.IsChildOf(game_object);
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
