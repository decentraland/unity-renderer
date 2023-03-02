using System;

namespace UIComponents.Scripts.Components.Tooltip
{
    [Serializable]
    public record TooltipComponentModel
    {
        public string message;

        public TooltipComponentModel()
        {
        }

        public TooltipComponentModel(string message)
        {
            this.message = message;
        }
    }
}
