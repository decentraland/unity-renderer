using System;

namespace DCL.SettingsCommon.SettingsControllers.BaseControllers
{
    /// <summary>
    /// This controller is in charge of manage all the base logic related to a TOGGLE CONTROL.
    /// </summary>
    public class ToggleSettingsControlController : SettingsControlController
    {
        public event Action<bool> ToggleValueChanged;

        protected void RaiseToggleValueChanged(bool newValue) =>
            ToggleValueChanged?.Invoke(newValue);
    }
}
