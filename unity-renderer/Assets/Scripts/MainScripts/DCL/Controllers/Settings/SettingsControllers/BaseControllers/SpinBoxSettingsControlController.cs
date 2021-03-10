using System;

namespace DCL.SettingsControls
{
    /// <summary>
    /// This controller is in charge of manage all the base logic related to a SPIN-BOX CONTROL.
    /// </summary>
    public class SpinBoxSettingsControlController : SettingsControlController
    {
        public event Action<string[]> OnSetLabels;
        protected void RaiseOnOverrideIndicatorLabel(string[] labels)
        {
            OnSetLabels?.Invoke(labels);
        }

        public event Action<string> OnCurrentLabelChange;
        protected void RaiseOnCurrentLabelChange(string newCurrentLabel)
        {
            OnCurrentLabelChange?.Invoke(newCurrentLabel);
        }
    }
}