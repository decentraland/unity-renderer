using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.SettingsPanelHUD.Controls
{
    /// <summary>
    /// MonoBehaviour that represents the view of a SLIDER type CONTROL.
    /// </summary>
    public class SliderSettingsControlView : SettingsControlView
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI indicatorLabel;

        public Slider sliderControl => slider;

        private SliderControlModel sliderControlConfig;

        public override void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController)
        {
            this.sliderControlConfig = (SliderControlModel)controlConfig;
            slider.maxValue = this.sliderControlConfig.sliderMaxValue;
            slider.minValue = this.sliderControlConfig.sliderMinValue;
            slider.wholeNumbers = this.sliderControlConfig.sliderWholeNumbers;

            base.Initialize(controlConfig, settingsControlController);
            OverrideIndicatorLabel(slider.value.ToString());
            settingsControlController.OnControlChanged(slider.value);

            slider.onValueChanged.AddListener(sliderValue =>
            {
                OverrideIndicatorLabel(sliderValue.ToString());
                ApplySetting(sliderValue);
            });
        }

        /// <summary>
        /// Overrides the text of the label associated to the slider.
        /// </summary>
        /// <param name="text">New label text.</param>
        public void OverrideIndicatorLabel(string text)
        {
            indicatorLabel.text = text;
        }

        public override void RefreshControl()
        {
            float newValue = (float)settingsControlController.GetStoredValue();
            if (slider.value != newValue)
                slider.value = newValue;
            else
                skipPostApplySettings = false;
        }
    }
}