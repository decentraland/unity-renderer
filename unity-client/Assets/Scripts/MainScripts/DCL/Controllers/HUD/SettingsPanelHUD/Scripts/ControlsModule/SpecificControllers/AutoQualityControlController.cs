using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Auto Quality", fileName = "AutoQualityControlController")]
    public class AutoQualityControlController : SettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentGeneralSettings.autoqualityOn;
        }

        public override void OnControlChanged(object newValue)
        {
            bool autoQualityValue = (bool)newValue;

            currentGeneralSettings.autoqualityOn = autoQualityValue;

            if (autoQualityValue)
            {
                SettingsData.QualitySettings.BaseResolution currentBaseResolution = currentQualitySetting.baseResolution;
                bool currentFpsCap = currentQualitySetting.fpsCap;
                currentQualitySetting = Settings.i.lastValidAutoqualitySet;
                currentQualitySetting.baseResolution = currentBaseResolution;
                currentQualitySetting.fpsCap = currentFpsCap;
            }
        }
    }
}