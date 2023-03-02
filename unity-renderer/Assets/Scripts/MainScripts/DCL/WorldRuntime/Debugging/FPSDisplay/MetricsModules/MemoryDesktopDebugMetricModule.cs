using System;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.FPSDisplay;
using UnityEngine.Profiling;

public class MemoryDesktopDebugMetricModule : IDebugMetricModule
{

    private const string NO_DECIMALS = "##";
    private const float BYTES_TO_MEGABYTES = 1048576f;
    
    public void SetUpModule(Dictionary<DebugValueEnum, Func<string>> updateValueDictionary)
    {
        updateValueDictionary.Add(DebugValueEnum.Memory_Total_Allocated, () =>  $"{(Profiler.GetTotalAllocatedMemoryLong() / BYTES_TO_MEGABYTES).ToString(NO_DECIMALS)} Mb"); 
        updateValueDictionary.Add(DebugValueEnum.Memory_Reserved_Ram, () => $"{(Profiler.GetTotalReservedMemoryLong() / BYTES_TO_MEGABYTES).ToString(NO_DECIMALS)} Mb"); 
        updateValueDictionary.Add(DebugValueEnum.Memory_Mono, () => $"{(Profiler.GetMonoUsedSizeLong() / BYTES_TO_MEGABYTES).ToString(NO_DECIMALS)} Mb");
        updateValueDictionary.Add(DebugValueEnum.Memory_Graphics_Card, () => $"{(Profiler.GetAllocatedMemoryForGraphicsDriver() / BYTES_TO_MEGABYTES).ToString(NO_DECIMALS)} Mb");
    }

    public void UpdateModule() { }
    public void EnableModule() {  }
    public void DisableModule() {  }

    public void Dispose() { }

}