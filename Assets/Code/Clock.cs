using UnityEngine;
using System.Collections;
using System;

[ExecuteAlways]
public class Clock : MonoBehaviour
{
    public string EpochDateString, LoadDateString;

    public float GameSpeed = 1;

    public DateTime Epoch { get { return Convert.ToDateTime(EpochDateString); } }
    public DateTime Now { get; set; }

    public double SecondsSinceEpoch
    { get { return DateToSecondsSinceEpoch(Now); } }

    void Start()
    {
        ReloadNow();
    }

    void Update()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            ReloadNow();
            return;
        }

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
