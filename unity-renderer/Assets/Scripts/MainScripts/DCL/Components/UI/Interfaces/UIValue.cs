using UnityEngine;

namespace DCL.Components
{
    [System.Serializable]
    public struct UIValue
    {
        public enum Unit
        {
            PERCENT,
            PIXELS
        }

        public float value;
        public Unit type;

        public void SetPixels(float value)
        {
            this.type = Unit.PIXELS;
            this.value = value;
        }

        public void SetPercent(float value)
        {
            this.type = Unit.PERCENT;
            this.value = value;
        }

        public UIValue(float value, Unit unitType = Unit.PIXELS)
        {
            this.value = value;
            this.type = unitType;
        }

        public float GetScaledValue(float parentSize)
        {
            if (type == Unit.PIXELS)
                return value;

            // Prevent division by zero
            if (parentSize <= Mathf.Epsilon)
                parentSize = 1;

            return value / 100 * parentSize;
        }
    }
}
