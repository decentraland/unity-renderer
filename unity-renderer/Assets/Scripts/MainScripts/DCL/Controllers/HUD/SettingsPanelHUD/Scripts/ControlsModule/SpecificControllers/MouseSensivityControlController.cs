using UnityEngine;
using UnityEngine.UI;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Mouse Sensivity", fileName = "MouseSensivityControlController")]
    public class MouseSensivityControlController : SettingsControlController
    {
        private Slider mouseSensitivitySlider;

        public override void Initialize(ISettingsControlView settingsControlView)
        {
            base.Initialize(settingsControlView);

            mouseSensitivitySlider = ((SliderSettingsControlView)view).sliderControl;
        }

        public override object GetStoredValue()
        {
            return Mathf.Lerp(mouseSensitivitySlider.minValue, mouseSensitivitySlider.maxValue, currentGeneralSettings.mouseSensitivity);
        }

        public override void OnControlChanged(object newValue)
        {
            currentGeneralSettings.mouseSensitivity = RemapMouseSensitivityTo01((float)newValue);
        }

        private float RemapMouseSensitivityTo01(float value)
        {
            return (value - mouseSensitivitySlider.minValue)
                / (mouseSensitivitySlider.maxValue - mouseSensitivitySlider.minValue)
                * (1 - 0) + 0; //(value - from1) / (to1 - from1) * (to2 - from2) + from2
        }
    }
}