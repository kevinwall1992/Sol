using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class Taskbar : UIElement
{
    public RectTransform TaskbarElementsContainer;

    public IEnumerable<TaskbarElement> TaskbarElements
    { get { return TaskbarElementsContainer.GetComponentsInChildren<TaskbarElement>(); } }

    public int Height { get { return RectTransform.sizeDelta.y.Round(); } }

    void Start()
    {

    }

    void Update()
    {
        IEnumerable<Window> windows_with_taskbar_elements = 
            TaskbarElements.Select(taskbar_element => taskbar_element.Window);

        foreach (Window window in Scene.The.WindowContainer.Windows)
            if (!windows_with_taskbar_elements.Contains(window))
                TaskbarElement.Create(window).transform.SetParent(TaskbarElementsContainer, false);

        Vector2Int offset = Vector2Int.zero;
        foreach(TaskbarElement taskbar_element in TaskbarElements)
        {
            taskbar_element.RectTransform.anchoredPosition = new Vector3(offset.x, offset.y, 0);

            offset += taskbar_element.RectTransform.rect.size.YChangedTo(0).ToVector2Int();
        }
    }
}
