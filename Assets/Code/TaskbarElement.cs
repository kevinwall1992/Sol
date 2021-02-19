using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteAlways]
public class TaskbarElement : UIElement
{
    public TMPro.TextMeshProUGUI TitleText;

    public Window Window { get; private set; }

    void Start()
    {

    }

    void Update()
    {
        RectTransform.sizeDelta = new Vector2(DefaultWidth, 
                                              The.Taskbar.Height - 4);

        if (!UnityEditor.EditorApplication.isPlaying)
            return;


        if (!Window.IsOpen)
        {
            GameObject.Destroy(gameObject);
            return;
        }

        TitleText.text = Window.TitleText.text;

        if (InputUtility.WasMouseLeftReleased && this.IsTouched())
        {
            if (!Window.IsMinimized && Window.IsTopmost)
                Window.IsMinimized = true;
            else
            {
                Window.IsMinimized = false;
                Window.MoveToFront();
            }
        }
    }

    public static TaskbarElement Create(Window window)
    {
        TaskbarElement taskbar_element = 
            GameObject.Instantiate(The.Prefabs.TaskbarElement);

        taskbar_element.Window = window;

        return taskbar_element;
    }


    public int DefaultWidth = 100;
}
