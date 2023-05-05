using System.Collections.Generic;

public static class GenericAnalytics
{
    public static void SendAnalytic(string eventName, Dictionary<string, string> data)
    {
        IAnalytics analytics = DCL.Environment.i?.platform?.serviceProviders?.analytics;
        analytics?.SendAnalytic(eventName, data);
    }

    public static void SendAnalytic(string eventName) { SendAnalytic(eventName, new Dictionary<string, string>()); }

}
