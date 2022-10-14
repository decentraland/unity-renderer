using System;
using System.Collections.Generic;
using UnityEngine;

public interface IDebugMetricModule : IDisposable
{

    public void SetUpModule(Dictionary<DebugValueEnum, Func<string>> updateValueDictionary);
    public void UpdateModule();
    public void EnableModule();
    public void DisableModule();

}
