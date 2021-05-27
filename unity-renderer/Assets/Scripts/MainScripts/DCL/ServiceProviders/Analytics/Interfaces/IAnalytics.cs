using System;
using System.Collections.Generic;

public interface IAnalytics : IDisposable
{
    void SendAnalytic(string eventName, Dictionary<string, string> data);
}