using System;
using UnityEngine;

namespace DCL.SettingsCommon
{
    public class PlayerPrefsAudioSettingsRepository : ISettingsRepository<AudioSettings>
    {
        private readonly IPlayerPrefsSettingsByKey settingsByKey;
        private readonly AudioSettings defaultSettings;
        private AudioSettings currentSettings;
        
        public event Action<AudioSettings> OnChanged;

        public PlayerPrefsAudioSettingsRepository(
            IPlayerPrefsSettingsByKey settingsByKey,
            AudioSettings defaultSettings)
        {
            this.settingsByKey = settingsByKey;
            this.defaultSettings = defaultSettings;
            currentSettings = Load();
        }

        public AudioSettings Data => currentSettings;

        public void Apply(AudioSettings settings)
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
            settingsByKey.SetBool("chatSFXEnabled", currentSettings.chatSFXEnabled);
            settingsByKey.SetFloat("masterVolume", currentSettings.masterVolume);
            settingsByKey.SetFloat("musicVolume", currentSettings.musicVolume);
            settingsByKey.SetFloat("voiceChatVolume", currentSettings.voiceChatVolume);
            settingsByKey.SetFloat("avatarSFXVolume", currentSettings.avatarSFXVolume);
            settingsByKey.SetFloat("sceneSFXVolume", currentSettings.sceneSFXVolume);
            settingsByKey.SetFloat("uiSFXVolume", currentSettings.uiSFXVolume);
        }

        public bool HasAnyData() => !Data.Equals(defaultSettings);

        private AudioSettings Load()
        {
            var settings = defaultSettings;
            
            try
            {
                settings.chatSFXEnabled = settingsByKey.GetBool("chatSFXEnabled", defaultSettings.chatSFXEnabled);
                settings.masterVolume = settingsByKey.GetFloat("masterVolume", defaultSettings.masterVolume);
                settings.musicVolume = settingsByKey.GetFloat("musicVolume", defaultSettings.musicVolume);
                settings.voiceChatVolume = settingsByKey.GetFloat("voiceChatVolume", defaultSettings.voiceChatVolume);
                settings.avatarSFXVolume = settingsByKey.GetFloat("avatarSFXVolume", defaultSettings.avatarSFXVolume);
                settings.sceneSFXVolume = settingsByKey.GetFloat("sceneSFXVolume", defaultSettings.sceneSFXVolume);
                settings.uiSFXVolume = settingsByKey.GetFloat("uiSFXVolume", defaultSettings.uiSFXVolume);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

            return settings;
        }
    }
}