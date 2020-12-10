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

        private SliderControlModel controlConfig;

        public override void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController)
        {
            this.controlConfig = (SliderControlModel)controlConfig;
            slider.maxValue = this.controlConfig.sliderMaxValue;
            slider.minValue = this.controlConfig.sliderMinValue;
            slider.wholeNumbers = this.controlConfig.sliderWholeNumbers;

            base.Initialize(controlConfig, settingsControlController);

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