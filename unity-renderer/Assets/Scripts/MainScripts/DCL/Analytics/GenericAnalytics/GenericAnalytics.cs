using System;
using System.Collections.Generic;

public static class GenericAnalytics
{
    public static void SendAnalytic(string eventName, Dictionary<string, string> data)
    {
        FillGenericData(data);

        IAnalytics analytics = DCL.Environment.i.platform.serviceProviders.analytics;
        analytics.SendAnalytic(eventName, data);
    }

    internal static void FillGenericData(Dictionary<string, string> data) { }
}