using System;
using DCL.Helpers;
using DCL.Interface;
using UnityEngine;
using UnityEngine.Audio;

namespace DCL.SettingsCommon
{
    public class Settings : Singleton<Settings>
    {
        const string QUALITY_SETTINGS_KEY = "Settings.Quality";
        const string GENERAL_SETTINGS_KEY = "Settings.General";
        const string AUDIO_SETTINGS_KEY = "Settings.Audio";
        public event Action OnResetAllSettings;
        public QualitySettingsData qualitySettingsPresets => qualitySettingsPreset;

        public ISettingsRepository<QualitySettings> qualitySettings;
        public ISettingsRepository<GeneralSettings> generalSettings;
        public ISettingsRepository<AudioSettings> audioSettings;

        private static QualitySettingsData qualitySettingsPreset = null;

        public QualitySettingsData autoqualitySettings = null;
        public QualitySettings lastValidAutoqualitySet;

        private readonly BooleanVariable autosettingsEnabled = null;
        private readonly AudioMixer audioMixer;

        public Settings()
        {
            if (qualitySettingsPreset == null)
            {
                qualitySettingsPreset = Resources.Load<QualitySettingsData>("ScriptableObjects/QualitySettingsData");
            }

            if (autoqualitySettings == null)
            {
                autoqualitySettings = Resources.Load<QualitySettingsData>("ScriptableObjects/AutoQualitySettingsData");
                lastValidAutoqualitySet = autoqualitySettings[autoqualitySettings.Length / 2];
            }

            if (autosettingsEnabled == null)
                autosettingsEnabled = Resources.Load<BooleanVariable>("ScriptableObjects/AutoQualityEnabled");

            qualitySettings = new ProxySettingsRepository<QualitySettings>(
                new PlayerPrefsQualitySettingsRepository(
                    new PlayerPrefsSettingsByKey(QUALITY_SETTINGS_KEY),
                    qualitySettingsPreset.defaultPreset),
                new SettingsModule<QualitySettings>(
                    QUALITY_SETTINGS_KEY,
                    qualitySettingsPreset.defaultPreset));
            generalSettings = new ProxySettingsRepository<GeneralSettings>(
                new PlayerPrefsGeneralSettingsRepository(
                    new PlayerPrefsSettingsByKey(GENERAL_SETTINGS_KEY),
                    GetDefaultGeneralSettings()),
                new SettingsModule<GeneralSettings>(
                    GENERAL_SETTINGS_KEY,
                    GetDefaultGeneralSettings()));
            audioSettings = new ProxySettingsRepository<AudioSettings>(
                new PlayerPrefsAudioSettingsRepository(
                    new PlayerPrefsSettingsByKey(AUDIO_SETTINGS_KEY),
                    GetDefaultAudioSettings()),
                new SettingsModule<AudioSettings>(
                    AUDIO_SETTINGS_KEY,
                    GetDefaultAudioSettings()));

            SubscribeToVirtualAudioMixerEvents();
            audioMixer = Resources.Load<AudioMixer>("AudioMixer");
        }

        public void Dispose() { UnsubscribeFromVirtualAudioMixerEvents(); }

        public void LoadDefaultSettings()
        {
            autosettingsEnabled.Set(false);

            qualitySettings.Reset();
            generalSettings.Reset();
            audioSettings.Reset();
        }

        public void ResetAllSettings()
        {
            LoadDefaultSettings();
            SaveSettings();
            OnResetAllSettings?.Invoke();
        }

        private GeneralSettings GetDefaultGeneralSettings()
        {
            return new GeneralSettings
            {
                mouseSensitivity = 0.6f,
                scenesLoadRadius = 4,
                avatarsLODDistance = 16,
                maxNonLODAvatars = DataStore_AvatarsLOD.DEFAULT_MAX_AVATAR,
                voiceChatVolume = 1,
                voiceChatAllow = GeneralSettings.VoiceChatAllow.ALL_USERS,
                autoqualityOn = false,
                namesOpacity = 0.5f,
                profanityChatFiltering = true,
                proceduralSkyboxMode = GeneralSettings.ProceduralSkyboxMode.DYNAMIC,
                skyboxTime = 0.0f,
            };
        }

        private AudioSettings GetDefaultAudioSettings()
        {
            return new AudioSettings()
            {
                masterVolume = 1f,
                voiceChatVolume = 1f,
                avatarSFXVolume = 1f,
                uiSFXVolume = 1f,
                sceneSFXVolume = 1f,
                musicVolume = 1f,
                chatSFXEnabled = true
            };
        }

        /// <summary>
        /// Apply the auto quality setting by its index on the array
        /// </summary>
        /// <param name="index">Index within the autoQualitySettings array</param>
        public void ApplyAutoQualitySettings(int index)
        {
            if (index < 0 || index >= autoqualitySettings.Length)
                return;

            lastValidAutoqualitySet = autoqualitySettings[index];

            var qualiltyData = qualitySettings.Data;

            lastValidAutoqualitySet.baseResolution = qualiltyData.baseResolution;
            lastValidAutoqualitySet.fpsCap = qualiltyData.fpsCap;

            qualitySettings.Apply(lastValidAutoqualitySet);
        }

        private void SubscribeToVirtualAudioMixerEvents()
        {
            DataStore.i.virtualAudioMixer.voiceChatVolume.OnChange += ApplyVoiceChatSettings;
            DataStore.i.virtualAudioMixer.musicVolume.OnChange += ApplyMusicVolume;
            DataStore.i.virtualAudioMixer.avatarSFXVolume.OnChange += ApplyAvatarSFXVolume;
            DataStore.i.virtualAudioMixer.uiSFXVolume.OnChange += ApplyUISFXVolume;
        }

        private void UnsubscribeFromVirtualAudioMixerEvents()
        {
            DataStore.i.virtualAudioMixer.voiceChatVolume.OnChange -= ApplyVoiceChatSettings;
            DataStore.i.virtualAudioMixer.musicVolume.OnChange -= ApplyMusicVolume;
            DataStore.i.virtualAudioMixer.avatarSFXVolume.OnChange -= ApplyAvatarSFXVolume;
            DataStore.i.virtualAudioMixer.uiSFXVolume.OnChange -= ApplyUISFXVolume;
        }

        public void ApplyMasterVolume()
        {
            // Update the "All" mixer group
            audioMixer.SetFloat("AllBusVolume", Utils.ToAudioMixerGroupVolume(audioSettings.Data.masterVolume));

            // Update voice chat volume, as it does not pass through the AudioMixer
            ApplyVoiceChatSettings();
        }

        public void ApplyVoiceChatSettings(float currentDataStoreVolume = 0f, float previousDataStoreVolume = 0f)
        {
            AudioSettings audioSettingsData = audioSettings.Data;
            float calculatedVolume = Utils.ToVolumeCurve(DataStore.i.virtualAudioMixer.voiceChatVolume.Get() * audioSettingsData.voiceChatVolume * audioSettingsData.masterVolume);
            WebInterface.ApplySettings(calculatedVolume, (int)generalSettings.Data.voiceChatAllow);
        }

        public void ApplyAvatarSFXVolume(float currentDataStoreVolume = 0f, float previousDataStoreVolume = 0f) { audioMixer.SetFloat("AvatarSFXBusVolume", Utils.ToAudioMixerGroupVolume(DataStore.i.virtualAudioMixer.avatarSFXVolume.Get() * audioSettings.Data.avatarSFXVolume)); }

        public void ApplyUISFXVolume(float currentDataStoreVolume = 0f, float previousDataStoreVolume = 0f) { audioMixer.SetFloat("UIBusVolume", Utils.ToAudioMixerGroupVolume(DataStore.i.virtualAudioMixer.uiSFXVolume.Get() * audioSettings.Data.uiSFXVolume)); }

        public void ApplyMusicVolume(float currentDataStoreVolume = 0f, float previousDataStoreVolume = 0f) { audioMixer.SetFloat("MusicBusVolume", Utils.ToAudioMixerGroupVolume(DataStore.i.virtualAudioMixer.uiSFXVolume.Get() * audioSettings.Data.musicVolume)); }

        public void SaveSettings()
        {
            generalSettings.Save();
            qualitySettings.Save();
            audioSettings.Save();
            PlayerPrefsUtils.Save();
        }
    }
}