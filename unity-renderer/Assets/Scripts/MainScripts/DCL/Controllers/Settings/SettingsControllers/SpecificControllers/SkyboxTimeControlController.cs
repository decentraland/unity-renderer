using DCL.SettingsCommon.SettingsControllers.BaseControllers;
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

            DataStore.i.skyboxConfig.mode.OnChange += OnSkyboxModeChanged;
        }

        private void OnSkyboxModeChanged(SkyboxMode current, SkyboxMode previous)
        {
            if (current == previous) return;

            RaiseSliderValueChanged(DataStore.i.skyboxConfig.fixedTime.Get());
            UpdateSetting(DataStore.i.skyboxConfig.fixedTime.Get());
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
