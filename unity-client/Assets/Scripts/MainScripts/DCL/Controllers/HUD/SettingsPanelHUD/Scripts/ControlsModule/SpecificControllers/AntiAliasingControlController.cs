using DCL.SettingsPanelHUD.Common;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/AntiAliasing", fileName = "AntiAliasingControlController")]
    public class AntiAliasingControlController : SettingsControlController
    {
        public const string TEXT_OFF = "OFF";

        private SliderSettingsControlView sliderView;

        public override void Initialize(ISettingsControlView settingsControlView)
        {
            base.Initialize(settingsControlView);

            sliderView = (SliderSettingsControlView)view;
        }

        public override object GetStoredValue()
        {
            float antiAliasingValue =
                currentQualitySetting.antiAliasing == UnityEngine.Rendering.Universal.MsaaQuality.Disabled
                    ? 0
                    : ((int)currentQualitySetting.antiAliasing >> 2) + 1;

            return antiAliasingValue;
        }

        public override void OnControlChanged(object newValue)
        {
            float newFloatValue = (float)newValue;

            int antiAliasingValue = 1 << (int)newFloatValue;
            currentQualitySetting.antiAliasing = (UnityEngine.Rendering.Universal.MsaaQuality)antiAliasingValue;

            if (newFloatValue == 0)
                sliderView.OverrideIndicatorLabel(TEXT_OFF);
            else
                sliderView.OverrideIndicatorLabel(antiAliasingValue.ToString("0x"));
        }

        public override void PostApplySettings()
        {
            base.PostApplySettings();

            CommonSettingsEvents.RaiseSetQualityPresetAsCustom();
        }
    }
}