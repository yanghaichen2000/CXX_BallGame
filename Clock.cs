using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock : MonoBehaviour
{

    [SerializeField]
    Transform hoursPivot, minutesPivot, secondsPivot;

    [SerializeField]
    float speed = 1.0f;

    const float hoursToDegrees = -30f, minutesToDegrees = -6f, secondsToDegrees = -6f;
    

    void Awake()
    {
        TimeSpan time = DateTime.Now.TimeOfDay;
        hoursPivot.localRotation =
            Quaternion.Euler(0f, 0f, hoursToDegrees * (float)time.TotalHours * speed);
        minutesPivot.localRotation =
            Quaternion.Euler(0f, 0f, minutesToDegrees * (float)time.TotalMinutes * speed);
        secondsPivot.localRotation =
            Quaternion.Euler(0f, 0f, secondsToDegrees * (float)time.TotalSeconds * speed);
    }

    void Update()
    {
        TimeSpan time = DateTime.Now.TimeOfDay;
        hoursPivot.localRotation =
            Quaternion.Euler(0f, 0f, hoursToDegrees * (float)time.TotalHours * speed);
        minutesPivot.localRotation =
            Quaternion.Euler(0f, 0f, minutesToDegrees * (float)time.TotalMinutes * speed);
        secondsPivot.localRotation =
            Quaternion.Euler(0f, 0f, secondsToDegrees * (float)time.TotalSeconds * speed);
    }
}