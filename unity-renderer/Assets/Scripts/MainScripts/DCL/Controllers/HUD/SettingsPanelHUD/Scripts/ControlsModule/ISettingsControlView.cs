// unset:none
using DCL.SettingsCommon.SettingsControllers.BaseControllers;

namespace DCL.SettingsPanelHUD.Controls
{
    /// <summary>
    /// Base interface to implement a view for a CONTROL.
    /// </summary>
    public interface ISettingsControlView
    {
        /// <summary>
        /// All the needed base logic to initializes the CONTROL view.
        /// </summary>
        /// <param name="controlConfig">Model that will contain the configuration of the CONTROL.</param>
        /// <param name="settingsControlController">Controller associated to the CONTROL view.</param>
        void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController);

        /// <summary>
        /// This logic should update the CONTROL view with the stored value.
        /// </summary>
        void RefreshControl();
    }
}
