using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : Singleton<TimeManager>
{
    private DateTime currentRun;

    void Start()
    {
        
    }

    void Update()
    {
        GetTimeDifference(DateTime.Now);
    }

    public DateTime GetCurrentDate()
    {
        return DateTime.Now;
    }

    /// <summary>
    /// Returns the current time minus the <c>dateTime</c> parameter time in milliseconds
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns>difference in milliseconds</returns>
    public int GetTimeDifference(DateTime dateTime)
    {
        TimeSpan diff = DateTime.Now - dateTime;
        return diff.Milliseconds;
    }


    public int GetTimeDifferenceInMs(DateTime start, DateTime current)
    {
        TimeSpan diff = current - start;
        return diff.Milliseconds;
    }

    public void StartCounting()
    {
        currentRun = DateTime.Now;
    }

    public int StopCounting()
    {
        return GetTimeDifferenceInMs(currentRun, DateTime.Now);
    }
}
