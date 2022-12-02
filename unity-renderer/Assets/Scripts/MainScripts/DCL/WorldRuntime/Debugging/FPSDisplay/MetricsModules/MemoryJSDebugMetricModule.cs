using System;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.FPSDisplay;
using DCL.Interface;

public class MemoryJSDebugMetricModule : IDebugMetricModule
{

    private const string NO_DECIMALS = "##";
    private const float BYTES_TO_MEGABYTES = 1048576f;

    private BaseVariable<float> jsUsedHeapSize => DataStore.i.debugConfig.jsUsedHeapSize;
    private BaseVariable<float> jsTotalHeapSize => DataStore.i.debugConfig.jsTotalHeapSize;
    private BaseVariable<float> jsHeapSizeLimit => DataStore.i.debugConfig.jsHeapSizeLimit;
    
    public void SetUpModule(Dictionary<DebugValueEnum, Func<string>> updateValueDictionary)
    {
        updateValueDictionary.Add(DebugValueEnum.Memory_Used_JS_Heap_Size, () =>  GetMemoryMetric(jsUsedHeapSize.Get() / BYTES_TO_MEGABYTES));
        updateValueDictionary.Add(DebugValueEnum.Memory_Total_JS_Heap_Size, () => GetMemoryMetric(jsTotalHeapSize.Get() / BYTES_TO_MEGABYTES));
        updateValueDictionary.Add(DebugValueEnum.Memory_Limit_JS_Heap_Size, () => GetMemoryMetric(jsHeapSizeLimit.Get() / BYTES_TO_MEGABYTES));
    }

    public void UpdateModule() { WebInterface.UpdateMemoryUsage(); }
    public void EnableModule() {  }
    public void DisableModule() { }

    private string GetMemoryMetric(float value) { return $"{FPSColoring.GetMemoryColoringString(value)}{value.ToString(NO_DECIMALS)} Mb</color>"; }

    public void Dispose() { }

}