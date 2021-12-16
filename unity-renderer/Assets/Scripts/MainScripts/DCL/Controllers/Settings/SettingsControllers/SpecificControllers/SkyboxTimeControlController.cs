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

            valueAsFloat = Mathf.Clamp(valueAsFloat, 0, 23.998f);

            currentGeneralSettings.skyboxTime = valueAsFloat;
            DataStore.i.skyboxConfig.fixedTime.Set(valueAsFloat);
            int hourSection = (int)valueAsFloat;
            float minuteSection = valueAsFloat - hourSection;
            minuteSection = minuteSection * 60;
            minuteSection = (int)minuteSection;

            string sliderTxt = hourSection.ToString("00") + ":" + minuteSection.ToString("00");

            RaiseOnIndicatorLabelChange(sliderTxt);
        }
    }
}