﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteAlways]
public class TaskbarElement : UIElement
{
    public TMPro.TextMeshProUGUI NameLabel;

    public Window Window { get; private set; }

    void Start()
    {

    }

    void Update()
    {
        RectTransform.sizeDelta = new Vector2(DefaultWidth, 
                                              Scene.The.Taskbar.Height - 4);

        if (!UnityEditor.EditorApplication.isPlaying)
            return;


        if (!Window.IsOpen)
        {
            GameObject.Destroy(gameObject);
            return;
        }

        NameLabel.text = Window.NameLabel.text;

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
            GameObject.Instantiate(Scene.The.Prefabs.TaskbarElement);

        taskbar_element.Window = window;

        return taskbar_element;
    }


    public int DefaultWidth = 100;
}
