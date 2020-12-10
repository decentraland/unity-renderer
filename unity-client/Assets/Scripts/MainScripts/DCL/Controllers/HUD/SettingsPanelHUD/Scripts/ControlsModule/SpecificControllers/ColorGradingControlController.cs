using DCL.SettingsPanelHUD.Common;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Color Grading", fileName = "ColorGradingControlController")]
    public class ColorGradingControlController : SettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentQualitySetting.colorGrading;
        }

        public override void OnControlChanged(object newValue)
        {
            currentQualitySetting.colorGrading = (bool)newValue;
        }

        public override void PostApplySettings()
        {
            base.PostApplySettings();

            CommonSettingsEvents.RaiseSetQualityPresetAsCustom();
        }
    }
}