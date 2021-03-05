using UnityEngine;
using System.Collections;
using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
using UnityEngine.Events;

public class Meeting : MonoBehaviour
{
    System.DateTime first_session;
    int session_count = 0;

    public float SessionsPerYear = 24;

    public float DaysBetweenSessions { get { return 365 / SessionsPerYear; } }

    private void Start()
    {
        first_session = The.Clock.Now.AddDays(DaysBetweenSessions * Random.value);
    }

    private void Update()
    {
        float days = (float)(The.Clock.Now - first_session).TotalDays;
        days -= DaysBetweenSessions * session_count;

        if (days >= 0)
        {
            session_count++;
            Commence();
        }
    }

    List<Task> tasks = new List<Task>();

    public void AddTask(Task task)
    {
        tasks.Add(task);
    }

    public void Commence()
    {
        foreach (Task task in tasks)
            task();
    }

    public delegate void Task();
}
