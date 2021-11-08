using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Auto Quality", fileName = "AutoQualityControlController")]
    public class AutoQualityControlController : ToggleSettingsControlController
    {
        public override object GetStoredValue() { return currentGeneralSettings.autoqualityOn; }

        public override void UpdateSetting(object newValue)
        {
            bool autoQualityValue = (bool)newValue;

            currentGeneralSettings.autoqualityOn = autoQualityValue;

            if (autoQualityValue)
            {
                QualitySettings.BaseResolution currentBaseResolution = currentQualitySetting.baseResolution;
                bool currentFpsCap = currentQualitySetting.fpsCap;
                currentQualitySetting = Settings.i.lastValidAutoqualitySet;
                currentQualitySetting.baseResolution = currentBaseResolution;
                currentQualitySetting.fpsCap = currentFpsCap;
            }
        }
    }
}