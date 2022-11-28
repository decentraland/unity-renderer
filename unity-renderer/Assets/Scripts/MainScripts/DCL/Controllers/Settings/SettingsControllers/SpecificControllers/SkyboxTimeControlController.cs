using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using System;
using UnityEngine;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Skybox Time", fileName = "SkyboxTime")]
    public class SkyboxTimeControlController : SliderSettingsControlController
    {
        public override void Initialize()
        {
            base.Initialize();
            UpdateSetting(currentGeneralSettings.skyboxTime);

            DataStore.i.skyboxConfig.fixedTime.OnChange += OnFixedTimeChanged;
        }

        private void OnFixedTimeChanged(float newTime, float _)
        {
            Debug.Log(newTime);
        }

        public override object GetStoredValue() =>
            currentGeneralSettings.skyboxTime;

        public override void UpdateSetting(object newValue)
        {
            var valueAsFloat = (float)newValue;

            valueAsFloat = Mathf.Clamp(valueAsFloat, 0, 23.998f);

            currentGeneralSettings.skyboxTime = valueAsFloat;

            var hourSection = (int)valueAsFloat;
            var minuteSection = (int)((valueAsFloat - hourSection) * 60);

            RaiseOnIndicatorLabelChange($"{hourSection:00}:{minuteSection:00}");

            DataStore.i.skyboxConfig.fixedTime.Set(valueAsFloat);
        }
    }
}
