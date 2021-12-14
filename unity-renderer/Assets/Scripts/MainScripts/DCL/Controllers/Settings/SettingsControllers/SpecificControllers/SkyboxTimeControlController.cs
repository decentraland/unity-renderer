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
        }

        public override object GetStoredValue() { return currentGeneralSettings.skyboxTime; }

        public override void UpdateSetting(object newValue)
        {
            float valueAsFloat = (float)newValue;
            currentGeneralSettings.skyboxTime = valueAsFloat;
            DataStore.i.skyboxConfig.fixedTime.Set(valueAsFloat);
            RaiseOnIndicatorLabelChange(valueAsFloat.ToString("0.0"));
        }
    }
}