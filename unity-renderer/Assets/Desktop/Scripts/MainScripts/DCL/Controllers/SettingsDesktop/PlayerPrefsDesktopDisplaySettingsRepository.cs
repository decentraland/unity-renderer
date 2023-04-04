using System;
using DCL.SettingsCommon;
using UnityEngine;

namespace MainScripts.DCL.Controllers.SettingsDesktop
{
    public class PlayerPrefsDesktopDisplaySettingsRepository : ISettingsRepository<DisplaySettings>
    {
        public const string VSYNC = "vSync";
        public const string RESOLUTION_SIZE_INDEX = "resolutionSizeIndexV2";
        public const string WINDOW_MODE = "windowMode";
        public const string FPS_CAP = "fpsCap";

        private readonly IPlayerPrefsSettingsByKey settingsByKey;
        private readonly DisplaySettings defaultSettings;
        private DisplaySettings currentSettings;

        public event Action<DisplaySettings> OnChanged;

        public PlayerPrefsDesktopDisplaySettingsRepository(
            IPlayerPrefsSettingsByKey settingsByKey,
            DisplaySettings defaultSettings)
        {
            this.settingsByKey = settingsByKey;
            this.defaultSettings = defaultSettings;
            currentSettings = Load();
        }

        public DisplaySettings Data => currentSettings;

        public void Apply(DisplaySettings settings)
        {
            if (currentSettings.Equals(settings)) return;
            currentSettings = settings;
            OnChanged?.Invoke(currentSettings);
        }

        public void Reset()
        {
            Apply(defaultSettings);
        }

        public void Save()
        {
            settingsByKey.SetBool(VSYNC, currentSettings.vSync);
            settingsByKey.SetInt(RESOLUTION_SIZE_INDEX, currentSettings.resolutionSizeIndex);
            settingsByKey.SetEnum(WINDOW_MODE, currentSettings.windowMode);
            settingsByKey.SetInt(FPS_CAP, currentSettings.fpsCapIndex);
        }

        public bool HasAnyData() =>
            !Data.Equals(defaultSettings);

        private DisplaySettings Load()
        {
            var settings = defaultSettings;

            try
            {
                settings.vSync = settingsByKey.GetBool(VSYNC, defaultSettings.vSync);
                settings.resolutionSizeIndex = settingsByKey.GetInt(RESOLUTION_SIZE_INDEX, -1);
                settings.windowMode = settingsByKey.GetEnum(WINDOW_MODE, defaultSettings.windowMode);
                settings.fpsCapIndex = settingsByKey.GetInt(FPS_CAP, defaultSettings.fpsCapIndex);
            }
            catch (Exception e) { Debug.LogException(e); }

            return settings;
        }
    }
}
