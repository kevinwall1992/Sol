using UnityEngine;
using System.Collections;

//For some reason, this script is necessary when changing 
//the .sizeDelta of an object's parent's RectTransform.
//Otherwise, the anchoredPosition resets back to zero.

[ExecuteAlways]
public class MaintainAnchoredPosition : UIElement
{
    public Vector2 AnchoredPosition;

    public bool Modifiable = true;

    private void Update()
    {
        if (UnityEditor.EditorApplication.isPlaying)
            Modifiable = false;

        if (Modifiable)
            AnchoredPosition = RectTransform.anchoredPosition;
        else
            RectTransform.anchoredPosition = AnchoredPosition;
    }
}
