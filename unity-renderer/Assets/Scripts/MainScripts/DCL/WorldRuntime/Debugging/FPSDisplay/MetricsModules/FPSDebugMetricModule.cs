using System;
using System.Collections.Generic;
using DCL;
using DCL.FPSDisplay;
using UnityEngine;

public class FPSDebugMetricModule : IDebugMetricModule
{
    
    private const string NO_DECIMALS = "##";
    private const string TWO_DECIMALS = "##.00";
    private float fps;
    private string fpsColor;
    private PerformanceMetricsDataVariable performanceData;

    public FPSDebugMetricModule(PerformanceMetricsDataVariable performanceData)
    {
        this.performanceData = performanceData;
    }

    public void SetUpModule(Dictionary<DebugValueEnum, Func<string>> updateValueDictionary)
    {
        updateValueDictionary.Add(DebugValueEnum.FPS, GetFPSCount);
        updateValueDictionary.Add(DebugValueEnum.FPS_HiccupsInTheLast1000, () => $"{fpsColor}{performanceData.Get().hiccupCount.ToString()}</color>");
        updateValueDictionary.Add(DebugValueEnum.FPS_HiccupsLoss, GetHiccupsLoss);
        updateValueDictionary.Add(DebugValueEnum.FPS_BadFramesPercentiles, () => $"{fpsColor}{((performanceData.Get().hiccupCount) / 10.0f).ToString(NO_DECIMALS)}%</color>");
    }
    public void UpdateModule()
    {
        fps = performanceData.Get().fpsCount;
        fpsColor = FPSColoring.GetDisplayColorString(fps);
    }
    public void EnableModule() {  }
    public void DisableModule() {  }

    private string GetFPSCount()
    {
        float dt = Time.unscaledDeltaTime;
        string fpsFormatted = fps.ToString("##");
        string msFormatted = (dt * 1000).ToString("##");
        return $"<b>FPS</b> {fpsColor}{fpsFormatted}</color> {msFormatted} ms";
    }
    
    private string GetHiccupsLoss()
    {
        return $"{fpsColor}{(100.0f * performanceData.Get().hiccupSum / performanceData.Get().totalSeconds).ToString(TWO_DECIMALS)}% {performanceData.Get().hiccupSum.ToString(TWO_DECIMALS)} in {performanceData.Get().totalSeconds.ToString(TWO_DECIMALS)} secs</color>";
    }

    public void Dispose() { }
}
