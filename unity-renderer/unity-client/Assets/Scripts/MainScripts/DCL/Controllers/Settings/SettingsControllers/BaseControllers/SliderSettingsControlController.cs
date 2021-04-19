using System;

namespace DCL.SettingsControls
{
    /// <summary>
    /// This controller is in charge of manage all the base logic related to a SLIDER CONTROL.
    /// </summary>
    public class SliderSettingsControlController : SettingsControlController
    {
        public event Action<string> OnIndicatorLabelChange;

        protected void RaiseOnIndicatorLabelChange(string newIndicatorLabel)
        {
            OnIndicatorLabelChange?.Invoke(newIndicatorLabel);
        }
    }
}