using MLAPI;
using MLAPI.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TimeManager : NetworkBehaviour
{
    private DateTime currentRun;
    public List<double> AllResults = new List<double>();

    void Update()
    {
        if (NetworkManager.Singleton.IsServer) return;
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("p");
            SendDoubleTimeServerRpc(DateTime.Now.ToOADate());
            //SendServerRpc(NetworkManager.Singleton.NetworkTime);
        }
    }

    public double CalculateSentTime(double startTime)
    {
        double wholePastTime = NetworkManager.Singleton.NetworkTime - startTime;
        AllResults.Add(wholePastTime);
        return wholePastTime;
    }

    public double CalculateAverageSendTime()
    {
        double addedResults = 0;
        foreach (var result in AllResults)
        {
            addedResults += result;
        }

        return addedResults / AllResults.Count;
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

    [ServerRpc(RequireOwnership = false)]
    public void SendDoubleTimeServerRpc(double time)
    {       
        DateTime gotTime = DateTime.FromOADate(time);
        TimeSpan pastTime = DateTime.Now - gotTime;
        Debug.Log(pastTime.TotalMilliseconds);

        SendClientRpc(pastTime.TotalMilliseconds, DateTime.Now.ToOADate());
    }

    [ClientRpc]
    private void SendClientRpc(double pastTime, double sendTime)
    {
        DateTime gotTime = DateTime.FromOADate(sendTime);
        TimeSpan wholePastTime = DateTime.Now - gotTime;
        Debug.Log(wholePastTime.TotalMilliseconds + pastTime);   
    }

}
