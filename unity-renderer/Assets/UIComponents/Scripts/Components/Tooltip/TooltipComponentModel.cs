using System;

namespace UIComponents.Scripts.Components.Tooltip
{
    [Serializable]
    public record TooltipComponentModel
    {
        public string message;
        public bool autoAdaptContainerSize = true;

        public TooltipComponentModel()
        {
        }

        public TooltipComponentModel(string message)
        {
            this.message = message;
            this.autoAdaptContainerSize = true;
        }
    }
}
