using UnityEngine;
using System.Collections;
using System;

[ExecuteAlways]
public class Clock : MonoBehaviour
{
    System.DateTime then;

    public string EpochDateString, LoadDateString;

    public float GameSpeed = 1;

    public DateTime Epoch { get { return Convert.ToDateTime(EpochDateString); } }
    public DateTime Now { get; set; }

    public double SecondsSinceEpoch
    { get { return DateToSecondsSinceEpoch(Now); } }

    public float DeltaTime
    { get { return (float)(Now - then).TotalSeconds; } }

    void Start()
    {
        ReloadNow();

        then = Now;
    }

    void Update()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            ReloadNow();
            return;
        }

        then = Now;
        Now = Now.AddSeconds(GameSpeed * Time.deltaTime);
    }

    void ReloadNow()
    {
        Now = Convert.ToDateTime(LoadDateString);
    }

    public double DateToSecondsSinceEpoch(DateTime date)
    {
        return (date - Epoch).TotalSeconds;
    }
}
