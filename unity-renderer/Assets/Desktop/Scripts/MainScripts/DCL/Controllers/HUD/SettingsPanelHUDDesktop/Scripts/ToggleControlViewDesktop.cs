using DCL.SettingsCommon;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using DCL.SettingsPanelHUD.Controls;
using MainScripts.DCL.Controllers.SettingsDesktop;

namespace MainScripts.DCL.Controllers.HUD.SettingsPanelHUDDesktop.Scripts
{
    public class ToggleControlViewDesktop : ToggleSettingsControlView
    {
        private ISettingsRepository<DisplaySettings> displaySettings => SettingsDesktop.SettingsDesktop.i.displaySettings;

        public override void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController)
        {
            base.Initialize(controlConfig, settingsControlController);
            displaySettings.OnChanged += OnSettingsChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            displaySettings.OnChanged -= OnSettingsChanged;
        }

        private void OnSettingsChanged(DisplaySettings newSettings)
        {
            RefreshControl();
        }
    }
}
