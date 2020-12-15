using DCL.SettingsPanelHUD.Common;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Rendering Scale", fileName = "RenderingScaleControlController")]
    public class RenderingScaleControlController : SettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentQualitySetting.renderScale;
        }

        public override void OnControlChanged(object newValue)
        {
            float renderingScaleValue = (float)newValue;

            currentQualitySetting.renderScale = renderingScaleValue;
            ((SliderSettingsControlView)view).OverrideIndicatorLabel(renderingScaleValue.ToString("0.0"));
        }

        public override void PostApplySettings()
        {
            base.PostApplySettings();

            CommonSettingsEvents.RaiseSetQualityPresetAsCustom();
        }
    }
}