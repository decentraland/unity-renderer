using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Reflection Resolution", fileName = "ReflectionResolutionControlController")]
    public class RefectionResolutionControlController : SpinBoxSettingsControlController
    {

        public override object GetStoredValue() { return (int)currentQualitySetting.reflectionResolution; }

        public override void UpdateSetting(object newValue)
        {
            int value = (int)newValue;
            currentQualitySetting.reflectionResolution = (QualitySettings.ReflectionResolution)value;

            switch (currentQualitySetting.reflectionResolution)
            {
                case QualitySettings.ReflectionResolution.Res_256:
                    DataStore.i.skyboxConfig.reflectionResolution.Set(256);
                    break;
                case QualitySettings.ReflectionResolution.Res_512:
                    DataStore.i.skyboxConfig.reflectionResolution.Set(512);
                    break;
                case QualitySettings.ReflectionResolution.Res_1024:
                    DataStore.i.skyboxConfig.reflectionResolution.Set(1024);
                    break;
                case QualitySettings.ReflectionResolution.Res_2048:
                    DataStore.i.skyboxConfig.reflectionResolution.Set(2048);
                    break;
                default:
                    DataStore.i.skyboxConfig.reflectionResolution.Set(256);
                    break;
            }
        }
    }
}