using System;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.FPSDisplay;

public class SkyboxDebugMetricModule : IDebugMetricModule
{

    private const string TWO_DECIMALS = "##.00";
    
    public void SetUpModule(Dictionary<DebugValueEnum, Func<string>> updateValueDictionary)
    {
        updateValueDictionary.Add(DebugValueEnum.Skybox_Config, () => DataStore.i.skyboxConfig.configToLoad.Get());
        updateValueDictionary.Add(DebugValueEnum.Skybox_Duration, () => $"{DataStore.i.skyboxConfig.lifecycleDuration.Get()}");
        updateValueDictionary.Add(DebugValueEnum.Skybox_GameTime, () => $"{DataStore.i.skyboxConfig.currentVirtualTime.Get().ToString(TWO_DECIMALS)}");
        updateValueDictionary.Add(DebugValueEnum.Skybox_UTCTime, () => $"{DataStore.i.worldTimer.GetCurrentTime()}");
    }

    public void UpdateModule() { }
    public void EnableModule() {  }
    public void DisableModule() { }

    public void Dispose() { }

}