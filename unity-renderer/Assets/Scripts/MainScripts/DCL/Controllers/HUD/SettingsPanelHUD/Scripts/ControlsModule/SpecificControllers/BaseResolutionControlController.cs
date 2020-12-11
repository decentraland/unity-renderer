using DCL.SettingsPanelHUD.Common;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Base Resolution", fileName = "BaseResolutionControlController")]
    public class BaseResolutionControlController : SettingsControlController
    {
        public override object GetStoredValue()
        {
            return (int)currentQualitySetting.baseResolution;
        }

        public override void OnControlChanged(object newValue)
        {
            currentQualitySetting.baseResolution = (SettingsData.QualitySettings.BaseResolution)newValue;
        }

        public override void PostApplySettings()
        {
            base.PostApplySettings();

            CommonSettingsEvents.RaiseSetQualityPresetAsCustom();
        }
    }
}