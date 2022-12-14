using System;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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

        private SliderControlModel sliderControlConfig;
        private SliderSettingsControlController sliderController;

        public override void Initialize(SettingsControlModel model, SettingsControlController controller)
        {
            sliderController = (SliderSettingsControlController)controller;
            sliderController.OnIndicatorLabelChange += OverrideIndicatorLabel;
            sliderController.SliderValueChanged += OverrideSliderValue;

            this.sliderControlConfig = (SliderControlModel)model;
            slider.maxValue = this.sliderControlConfig.sliderMaxValue;
            slider.minValue = this.sliderControlConfig.sliderMinValue;
            slider.wholeNumbers = this.sliderControlConfig.wholeNumbers;

            base.Initialize(model, sliderController);
            OverrideIndicatorLabel(slider.value.ToString());
            sliderController.UpdateSetting(this.sliderControlConfig.storeValueAsNormalized ? RemapSliderValueTo01(slider.value) : slider.value);

            slider.onValueChanged.AddListener(OnSliderValueChanged());
        }

        private UnityAction<float> OnSliderValueChanged()
        {
            return sliderValue =>
            {
                // https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings
                // FX = floating point with X precision digits
                string stringFormat = sliderControlConfig.wholeNumbers ? "F0" : "F1";

                OverrideIndicatorLabel(sliderValue.ToString(stringFormat));
                ApplySetting(this.sliderControlConfig.storeValueAsNormalized ? RemapSliderValueTo01(sliderValue) : sliderValue);
            };
        }

        private void OverrideSliderValue(float newValue)
        {
            slider.onValueChanged.RemoveListener(OnSliderValueChanged());
            slider.value = newValue;
            slider.onValueChanged.AddListener(OnSliderValueChanged());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            slider.onValueChanged.RemoveListener(OnSliderValueChanged());

            if (sliderController != null)
                sliderController.OnIndicatorLabelChange -= OverrideIndicatorLabel;
        }

        /// <summary>
        /// Overrides the text of the label associated to the slider.
        /// </summary>
        /// <param name="text">New label text.</param>
        public void OverrideIndicatorLabel(string text) =>
            indicatorLabel.text = text;

        public override void RefreshControl()
        {
            base.RefreshControl();

            var storedValue = Convert.ToSingle(sliderController.GetStoredValue());

            float newValue = sliderControlConfig.storeValueAsNormalized ? RemapNormalizedValueToSlider(storedValue) : storedValue;

            if (slider.value != newValue)
                slider.value = newValue;
        }

        private float RemapSliderValueTo01(float value) =>
            ((value - slider.minValue)
             / (slider.maxValue - slider.minValue)
             * (1 - 0)) + 0; //(value - from1) / (to1 - from1) * (to2 - from2) + from2

        private float RemapNormalizedValueToSlider(float value) =>
            Mathf.Lerp(slider.minValue, slider.maxValue, value);
    }
}
