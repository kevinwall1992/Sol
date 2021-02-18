using UnityEngine;
using System.Collections;

public class ClickDetector : MonoBehaviour
{
    public bool IsPressed { get; private set; }

    bool was_clicked_in_most_recent_update;
    public bool WasClicked
    {
        get
        {
            return (was_clicked_in_most_recent_update &&
                   InputUtility.WasMouseLeftReleased) ||

                   (IsPressed &&
                   InputUtility.WasMouseLeftReleased);
        }
    }

    private void Update()
    {
        if (InputUtility.WasMouseLeftPressed && this.IsTouched())
            IsPressed = true;

        was_clicked_in_most_recent_update = false;
        if (InputUtility.WasMouseLeftReleased)
        {
            if (IsPressed && this.IsTouched())
                was_clicked_in_most_recent_update = true;

            IsPressed = false;
        }
    }
}
