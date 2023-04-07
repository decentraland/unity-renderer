using System;
using UnityEngine;

namespace DCL.SettingsCommon
{
    public class PlayerPrefsAudioSettingsRepository : ISettingsRepository<AudioSettings>
    {
        private const string CHAT_NOTIFICATIONS_TYPE = "chatNotificationsType";
        private const string MASTER_VOLUME = "masterVolume";
        private const string MUSIC_VOLUME = "musicVolume";
        private const string VOICE_CHAT_VOLUME = "voiceChatVolume";
        private const string AVATAR_SFX_VOLUME = "avatarSFXVolume";
        private const string SCENE_SFX_VOLUME = "sceneSFXVolume";
        private const string UI_SFX_VOLUME = "uiSFXVolume";

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
            settingsByKey.SetEnum(CHAT_NOTIFICATIONS_TYPE, currentSettings.chatNotificationType);
            settingsByKey.SetFloat(MASTER_VOLUME, currentSettings.masterVolume);
            settingsByKey.SetFloat(MUSIC_VOLUME, currentSettings.musicVolume);
            settingsByKey.SetFloat(VOICE_CHAT_VOLUME, currentSettings.voiceChatVolume);
            settingsByKey.SetFloat(AVATAR_SFX_VOLUME, currentSettings.avatarSFXVolume);
            settingsByKey.SetFloat(SCENE_SFX_VOLUME, currentSettings.sceneSFXVolume);
            settingsByKey.SetFloat(UI_SFX_VOLUME, currentSettings.uiSFXVolume);
        }

        public bool HasAnyData() => !Data.Equals(defaultSettings);

        private AudioSettings Load()
        {
            var settings = defaultSettings;

            try
            {
                settings.chatNotificationType = settingsByKey.GetEnum(CHAT_NOTIFICATIONS_TYPE, defaultSettings.chatNotificationType);
                settings.masterVolume = settingsByKey.GetFloat(MASTER_VOLUME, defaultSettings.masterVolume);
                settings.musicVolume = settingsByKey.GetFloat(MUSIC_VOLUME, defaultSettings.musicVolume);
                settings.voiceChatVolume = settingsByKey.GetFloat(VOICE_CHAT_VOLUME, defaultSettings.voiceChatVolume);
                settings.avatarSFXVolume = settingsByKey.GetFloat(AVATAR_SFX_VOLUME, defaultSettings.avatarSFXVolume);
                settings.sceneSFXVolume = settingsByKey.GetFloat(SCENE_SFX_VOLUME, defaultSettings.sceneSFXVolume);
                settings.uiSFXVolume = settingsByKey.GetFloat(UI_SFX_VOLUME, defaultSettings.uiSFXVolume);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return settings;
        }
    }
}
