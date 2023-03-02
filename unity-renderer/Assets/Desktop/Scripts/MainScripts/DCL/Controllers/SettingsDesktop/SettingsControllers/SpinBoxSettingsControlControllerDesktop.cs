using DCL.SettingsCommon;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;

namespace MainScripts.DCL.Controllers.SettingsDesktop.SettingsControllers
{
    public class SpinBoxSettingsControlControllerDesktop : SpinBoxSettingsControlController
    {
        protected DisplaySettings currentDisplaySettings;
        private ISettingsRepository<DisplaySettings> DisplaySettings => SettingsDesktop.i.displaySettings;

        public override void Initialize()
        {
            currentDisplaySettings = DisplaySettings.Data;
            DisplaySettings.OnChanged += OnDesktopSettingsChanged;
            base.Initialize();
        }

        private void OnDesktopSettingsChanged(DisplaySettings settings)
        {
            currentDisplaySettings = settings;
        }

        public override void ApplySettings()
        {
            base.ApplySettings();
            DisplaySettings.Apply(currentDisplaySettings);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            DisplaySettings.OnChanged -= OnDesktopSettingsChanged;
        }
    }
}
