using TMPro;
using UnityEngine;

namespace UIComponents.Scripts.Components.RangeSlider
{
    public class RangeSliderComponentView : BaseComponentView<RangeSliderComponentModel>
    {
        [Header("Prefab References")]
        [SerializeField] internal TMP_Text text;
        [SerializeField] public RangeSlider slider;

        public float MinValue => slider.MinValue;
        public float MaxValue => slider.MaxValue;
        public float LowValue => slider.LowValue;
        public float HighValue => slider.HighValue;
        public RangeSlider.RangeSliderEvent OnValueChanged => slider.OnValueChanged;

        public override void RefreshControl()
        {
            if (model == null)
                return;

            SetText(model.text);
            SetLimits(model.minValue, model.maxValue);
            SetWholeNumbers(model.wholeNumbers);
            SetValues(model.lowValue, model.highValue);
        }

        public void SetText(string newText)
        {
            model.text = newText;

            if (text == null)
                return;

            text.text = newText;
        }

        public void SetLimits(float minValue, float maxValue)
        {
            maxValue = maxValue > minValue ? maxValue : minValue;

            model.minValue = minValue;
            model.maxValue = maxValue;

            if (slider == null)
                return;

            slider.MinValue = minValue;
            slider.MaxValue = maxValue;
        }

        public void SetWholeNumbers(bool isWholeNumbers)
        {
            model.wholeNumbers = isWholeNumbers;

            if (slider == null)
                return;

            slider.WholeNumbers = isWholeNumbers;
        }

        public void SetValues(float lowValue, float highValue)
        {
            lowValue = lowValue >= model.minValue ? lowValue : model.minValue;
            highValue = highValue > lowValue ? highValue : lowValue;
            highValue = highValue <= model.maxValue ? highValue : model.maxValue;

            model.lowValue = lowValue;
            model.highValue = highValue;

            if (slider == null)
                return;

            slider.LowValue = lowValue;
            slider.HighValue = highValue;
        }
    }
}
