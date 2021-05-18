using System;
using System.Collections.Generic;
using DCL.Interface;

public interface IAnalytics : IDisposable
{
    void SendAnalytic(string eventName, Dictionary<object, object> data);
}

public class Analytics : IAnalytics
{
    //Remove this once environment is on its own assembly and can be accessed properly
    public static IAnalytics i;
    public Analytics() { i = this; }

    public void SendAnalytic(string eventName, Dictionary<object, object> data) { SendToSegment(eventName, data); }

    internal void SendToSegment(string eventName, Dictionary<object, object> data) { WebInterface.SendGenericAnalytic(eventName, data); }

    public void Dispose() { }
}