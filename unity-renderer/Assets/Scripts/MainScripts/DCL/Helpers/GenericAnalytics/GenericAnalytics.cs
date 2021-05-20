using System.Collections.Generic;

public static class GenericAnalytics
{
    public static void SendAnalytic(string eventName, Dictionary<string, string> data)
    {
        FillGenericData(data);

        //TODO wait until environment it's on its own assembly
        //Environment.i.platform.serviceProviders.analytics;
        Analytics.i.SendAnalytic(eventName, data);
    }

    internal static void FillGenericData(Dictionary<string, string> data) { }
}