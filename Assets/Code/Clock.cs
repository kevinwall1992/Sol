using UnityEngine;
using System.Collections;
using System;

public class Clock : MonoBehaviour
{
    DateTime start_time, current_time;

    public float GameSpeed = 1;

    public float Seconds
    { get { return (float)((current_time - start_time).TotalSeconds); } }

    void Start()
    {
        start_time = new DateTime(2010, 1, 1);
        current_time = new DateTime(2033, 1, 1);
    }

    void Update()
    {
        current_time = current_time.AddSeconds(GameSpeed * Time.deltaTime);
    }  
}
