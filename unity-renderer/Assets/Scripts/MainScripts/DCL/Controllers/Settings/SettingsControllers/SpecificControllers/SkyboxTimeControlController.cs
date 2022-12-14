using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Skybox Time", fileName = "SkyboxTime")]
    public class SkyboxTimeControlController : SliderSettingsControlController
    {
        private readonly DataStore_SkyboxConfig skyboxConfig = DataStore.i.skyboxConfig;

        public override void Initialize()
        {
            base.Initialize();
            UpdateSetting(currentGeneralSettings.skyboxTime);

            skyboxConfig.mode.OnChange += OnSkyboxModeChanged;
        }

        private void OnSkyboxModeChanged(SkyboxMode current, SkyboxMode previous)
        {
            if (current == previous) return;

            RaiseSliderValueChanged((float)GetStoredValue());
            UpdateSetting(GetStoredValue());
        }

        public override object GetStoredValue() =>
            skyboxConfig.mode.Equals(SkyboxMode.Dynamic)
                ? (object)currentGeneralSettings.skyboxTime
                : skyboxConfig.fixedTime.Get();

        public override void UpdateSetting(object newValue)
        {
            var valueAsFloat = (float)newValue;

            valueAsFloat = Mathf.Clamp(valueAsFloat, 0, 23.998f);

            currentGeneralSettings.skyboxTime = valueAsFloat;

            var hourSection = (int)valueAsFloat;
            var minuteSection = (int)((valueAsFloat - hourSection) * 60);

            RaiseOnIndicatorLabelChange($"{hourSection:00}:{minuteSection:00}");

            skyboxConfig.fixedTime.Set(valueAsFloat);
        }
    }
}
