using UnityEngine;
using System.Collections;
using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
using UnityEngine.Events;

public class Meeting : MonoBehaviour
{
    System.DateTime first_session_date, last_completed_session_date;
    int session_count = 0;

    public float SessionsPerYear = 24;

    public float DaysBetweenSessions
    { get { return 365 / SessionsPerYear; } }

    public System.DateTime LastCompletedSessionDate
    { get { return last_completed_session_date; } }

    private void Start()
    {
        first_session_date = The.Clock.Now.AddDays(DaysBetweenSessions * Random.value);
    }

    private void Update()
    {
        float days = (float)(The.Clock.Now - first_session_date).TotalDays;
        days -= DaysBetweenSessions * session_count;

        if (days >= 0)
        {
            Commence();

            last_completed_session_date = The.Clock.Now;
            session_count++;
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

        last_completed_session_date = The.Clock.Now;
    }

    public delegate void Task();
}
