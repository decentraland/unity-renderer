using System.Collections.Generic;
using UnityEngine;

public static class GenericAnalytics
{
    public static void SendAnalytic(string eventName, Dictionary<string, string> data)
    {
        FillGenericData(data);

        //TODO wait until environment it's on its own assembly
        //Environment.i.platform.serviceProviders.analytics;
        Analytics.i.SendAnalytic(eventName, data);

        Debug.Log("SANTI | ----------------------- SEND ANALYTIC -----------------------");
        Debug.Log("SANTI | eventName: " + eventName);
        foreach (var item in data)
        {
            Debug.Log("SANTI | =====> " + item.Key + ": " + item.Value);
        }
        Debug.Log("SANTI |-------------------------------------------------------------");
    }

    internal static void FillGenericData(Dictionary<string, string> data) { }
}