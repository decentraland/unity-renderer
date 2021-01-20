using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    /// <summary>
    /// Model that represents a SLIDER type CONTROL.
    /// </summary>
    [CreateAssetMenu(menuName = "Settings/Configuration/Controls/Slider Control", fileName = "SliderControlConfiguration")]
    public class SliderControlModel : SettingsControlModel
    {
        [Header("SLIDER CONFIGURATION")]
        [Tooltip("Min allowed value for the slider.")]
        public float sliderMinValue;

        [Tooltip("Max allowed value for the slider.")]
        public float sliderMaxValue;

        [Tooltip("True if the slider values will be integers.")]
        public bool sliderWholeNumbers;

        [Tooltip("True if the slider value will be stored in a normalized way.")]
        public bool storeValueAsNormalized;
    }
}