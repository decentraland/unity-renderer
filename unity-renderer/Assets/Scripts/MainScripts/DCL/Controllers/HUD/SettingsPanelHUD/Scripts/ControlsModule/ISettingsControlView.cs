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
        /// <param name="model">Model that will contain the configuration of the CONTROL.</param>
        /// <param name="controller">Controller associated to the CONTROL view.</param>
        void Initialize(SettingsControlModel model, SettingsControlController controller);

        /// <summary>
        /// This logic should update the CONTROL view with the stored value.
        /// </summary>
        void RefreshControl();
    }
}
