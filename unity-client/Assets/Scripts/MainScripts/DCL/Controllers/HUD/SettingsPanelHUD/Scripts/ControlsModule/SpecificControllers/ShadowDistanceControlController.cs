using DCL.SettingsPanelHUD.Common;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Shadow Distance", fileName = "ShadowDistanceControlController")]
    public class ShadowDistanceControlController : SettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentQualitySetting.shadowDistance;
        }

        public override void OnControlChanged(object newValue)
        {
            currentQualitySetting.shadowDistance = (float)newValue;
        }

        public override void PostApplySettings()
        {
            base.PostApplySettings();

            CommonSettingsEvents.RaiseSetQualityPresetAsCustom();
        }
    }
}