using System;
using UnityEngine;

namespace DCL.SettingsCommon
{
    public class PlayerPrefsGeneralSettingsRepository : ISettingsRepository<GeneralSettings>
    {
        private readonly IPlayerPrefsSettingsByKey settingsByKey;
        private readonly GeneralSettings defaultSettings;
        private GeneralSettings currentSettings;
        
        public event Action<GeneralSettings> OnChanged;

        public PlayerPrefsGeneralSettingsRepository(
            IPlayerPrefsSettingsByKey settingsByKey,
            GeneralSettings defaultSettings)
        {
            this.settingsByKey = settingsByKey;
            this.defaultSettings = defaultSettings;
            currentSettings = Load();
        }

        public GeneralSettings Data => currentSettings;

        public void Apply(GeneralSettings settings)
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
            settingsByKey.SetBool("autoqualityOn", currentSettings.autoqualityOn);
            settingsByKey.SetBool("profanityChatFiltering", currentSettings.profanityChatFiltering);
            settingsByKey.SetFloat("mouseSensitivity", currentSettings.mouseSensitivity);
            settingsByKey.SetFloat("namesOpacity", currentSettings.namesOpacity);
            settingsByKey.SetFloat("scenesLoadRadius", currentSettings.scenesLoadRadius);
            settingsByKey.SetFloat("voiceChatVolume", currentSettings.voiceChatVolume);
            settingsByKey.SetFloat("avatarsLODDistance", currentSettings.avatarsLODDistance);
            settingsByKey.SetFloat("maxNonLODAvatars", currentSettings.maxNonLODAvatars);
            settingsByKey.SetEnum("voiceChatAllow", currentSettings.voiceChatAllow);
        }

        public bool HasAnyData() => !Data.Equals(defaultSettings);

        private GeneralSettings Load()
        {
            var settings = defaultSettings;
            
            try
            {
                settings.autoqualityOn = settingsByKey.GetBool("autoqualityOn", defaultSettings.autoqualityOn);
                settings.profanityChatFiltering = settingsByKey.GetBool("profanityChatFiltering",
                    defaultSettings.profanityChatFiltering);
                settings.mouseSensitivity = settingsByKey.GetFloat("mouseSensitivity", defaultSettings.mouseSensitivity);
                settings.namesOpacity = settingsByKey.GetFloat("namesOpacity", defaultSettings.namesOpacity);
                settings.scenesLoadRadius = settingsByKey.GetFloat("scenesLoadRadius", defaultSettings.scenesLoadRadius);
                settings.voiceChatVolume = settingsByKey.GetFloat("voiceChatVolume", defaultSettings.voiceChatVolume);
                settings.avatarsLODDistance = settingsByKey.GetFloat("avatarsLODDistance", defaultSettings.avatarsLODDistance);
                settings.maxNonLODAvatars = settingsByKey.GetFloat("maxNonLODAvatars", defaultSettings.maxNonLODAvatars);
                settings.voiceChatAllow = settingsByKey.GetEnum("voiceChatAllow", defaultSettings.voiceChatAllow);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

            return settings;
        }
    }
}