using DCL.SettingsControls;
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
        private SliderSettingsControlController sliderController;

        public override void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController)
        {
            this.sliderControlConfig = (SliderControlModel)controlConfig;
            slider.maxValue = this.sliderControlConfig.sliderMaxValue;
            slider.minValue = this.sliderControlConfig.sliderMinValue;
            slider.wholeNumbers = this.sliderControlConfig.sliderWholeNumbers;

            sliderController = (SliderSettingsControlController)settingsControlController;
            sliderController.OnIndicatorLabelChange += OverrideIndicatorLabel;

            base.Initialize(controlConfig, sliderController);
            OverrideIndicatorLabel(slider.value.ToString());
            sliderController.UpdateSetting(this.sliderControlConfig.storeValueAsNormalized ? RemapSliderValueTo01(slider.value) : slider.value);

            slider.onValueChanged.AddListener(sliderValue =>
            {
                OverrideIndicatorLabel(sliderValue.ToString());
                ApplySetting(this.sliderControlConfig.storeValueAsNormalized ? RemapSliderValueTo01(sliderValue) : sliderValue);
            });
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (sliderController != null)
                sliderController.OnIndicatorLabelChange -= OverrideIndicatorLabel;
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
            base.RefreshControl();

            float storedValue = (float)sliderController.GetStoredValue();
            float newValue = sliderControlConfig.storeValueAsNormalized ? RemapNormalizedValueToSlider(storedValue) : storedValue;
            if (slider.value != newValue)
                slider.value = newValue;
        }

        private float RemapSliderValueTo01(float value)
        {
            return (value - slider.minValue)
                / (slider.maxValue - slider.minValue)
                * (1 - 0) + 0; //(value - from1) / (to1 - from1) * (to2 - from2) + from2
        }

        private float RemapNormalizedValueToSlider(float value)
        {
            return Mathf.Lerp(slider.minValue, slider.maxValue, value);
        }
    }
}