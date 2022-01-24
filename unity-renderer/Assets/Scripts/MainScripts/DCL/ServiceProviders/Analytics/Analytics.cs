using System.Collections.Generic;
using System.Linq;
using DCL.Interface;
using Newtonsoft.Json;

/// <summary>
/// For the events we use the convention of all lower cases and "_" instead of space
/// </summary>
public class Analytics : IAnalytics
{
    private static bool VERBOSE = false;

    public void SendAnalytic(string eventName, Dictionary<string, string> data)
    {
        if (VERBOSE)
            UnityEngine.Debug.Log($"{eventName}:\n{JsonConvert.SerializeObject(data, Formatting.Indented)}");

        SendToSegment(eventName, data);
    }

    internal void SendToSegment(string eventName, Dictionary<string, string> data) { WebInterface.ReportAnalyticsEvent(eventName, data.Select(x => new WebInterface.AnalyticsPayload.Property(x.Key, x.Value)).ToArray()); }

    public void Dispose() { }
}