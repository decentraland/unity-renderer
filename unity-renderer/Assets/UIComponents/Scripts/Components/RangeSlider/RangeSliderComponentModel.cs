using System;

namespace UIComponents.Scripts.Components.RangeSlider
{
    [Serializable]
    public record RangeSliderComponentModel
    {
        public string text;
        public float minValue;
        public float maxValue;
        public float lowValue;
        public float highValue;
    }
}
