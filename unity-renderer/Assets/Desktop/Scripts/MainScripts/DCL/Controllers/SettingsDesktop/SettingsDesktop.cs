using DCL;
using DCL.Helpers;
using DCL.SettingsCommon;

namespace MainScripts.DCL.Controllers.SettingsDesktop
{
    public class SettingsDesktop : Singleton<SettingsDesktop>
    {
        const string DESKTOP_SETTINGS_KEY = "Settings.Quality.Desktop";

        public readonly ISettingsRepository<DisplaySettings> displaySettings;

        public SettingsDesktop()
        {
            displaySettings = new ProxySettingsRepository<DisplaySettings>(
                new PlayerPrefsDesktopDisplaySettingsRepository(new PlayerPrefsSettingsByKey(DESKTOP_SETTINGS_KEY), GetDefaultDisplaySettings()),
                new SettingsModule<DisplaySettings>(DESKTOP_SETTINGS_KEY, GetDefaultDisplaySettings()));
        }

        private DisplaySettings GetDefaultDisplaySettings()
        {
            return new DisplaySettings
            {
                windowMode = WindowMode.Borderless,
                resolutionSizeIndex = 0,
                vSync = false,
                fpsCapIndex = 0
            };
        }

        public void Save()
        {
            displaySettings.Save();
            PlayerPrefsBridge.Save();
            PlayerPrefsBridge.Save();
        }
    }
}
