using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Button))]
public class ButtonLabel : MonoBehaviour
{
    public Label Label;

    public Button Button { get { return GetComponent<Button>(); } }

    private void Update()
    {
        Label.BackgroundColor = Button.GetDesiredColor();
    }
}
