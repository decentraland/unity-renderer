using UnityEngine;
using System;
using DCL.Helpers;
using DCL.Interface;
using UnityEngine.Audio;

namespace DCL
{
    public class Settings : DCL.Singleton<Settings>
    {
        const string QUALITY_SETTINGS_KEY = "Settings.Quality";
        const string GENERAL_SETTINGS_KEY = "Settings.General";
        const string AUDIO_SETTINGS_KEY = "Settings.Audio";

        public event Action<SettingsData.QualitySettings> OnQualitySettingsChanged;
        public event Action<SettingsData.GeneralSettings> OnGeneralSettingsChanged;
        public event Action<SettingsData.AudioSettings> OnAudioSettingsChanged;
        public event Action OnResetAllSettings;

        public SettingsData.QualitySettings qualitySettings { get { return currentQualitySettings; } }
        public SettingsData.QualitySettingsData qualitySettingsPresets { get { return qualitySettingsPreset; } }
        public SettingsData.GeneralSettings generalSettings { get { return currentGeneralSettings; } }
        public SettingsData.AudioSettings audioSettings { get { return currentAudioSettings; } }

        private static SettingsData.QualitySettingsData qualitySettingsPreset = null;

        public SettingsData.QualitySettingsData autoqualitySettings = null;
        public SettingsData.QualitySettings lastValidAutoqualitySet;

        private readonly BooleanVariable autosettingsEnabled = null;

        public SettingsData.QualitySettings currentQualitySettings { private set; get; }
        private SettingsData.GeneralSettings currentGeneralSettings;
        public SettingsData.AudioSettings currentAudioSettings { private set; get; }

        AudioMixer audioMixer;

        public Settings()
        {
            if (qualitySettingsPreset == null)
            {
                qualitySettingsPreset = Resources.Load<SettingsData.QualitySettingsData>("ScriptableObjects/QualitySettingsData");
            }

            if (autoqualitySettings == null)
            {
                autoqualitySettings = Resources.Load<SettingsData.QualitySettingsData>("ScriptableObjects/AutoQualitySettingsData");
                lastValidAutoqualitySet = autoqualitySettings[autoqualitySettings.Length / 2];
            }

            if (autosettingsEnabled == null)
                autosettingsEnabled = Resources.Load<BooleanVariable>("ScriptableObjects/AutoQualityEnabled");

            LoadQualitySettings();
            LoadGeneralSettings();
            LoadAudioSettings();

            SubscribeToVirtualAudioMixerEvents();
            audioMixer = Resources.Load<AudioMixer>("AudioMixer");
        }

        public void Dispose() { UnsubscribeFromVirtualAudioMixerEvents(); }

        private void LoadQualitySettings()
        {
            bool isQualitySettingsSet = false;
            if (PlayerPrefsUtils.HasKey(QUALITY_SETTINGS_KEY))
            {
                try
                {
                    currentQualitySettings = JsonUtility.FromJson<SettingsData.QualitySettings>(PlayerPrefsUtils.GetString(QUALITY_SETTINGS_KEY));
                    isQualitySettingsSet = true;
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }

            if (!isQualitySettingsSet)
            {
                currentQualitySettings = qualitySettingsPreset.defaultPreset;
            }
        }

        private void LoadGeneralSettings()
        {
            currentGeneralSettings = GetDefaultGeneralSettings();

            if (PlayerPrefsUtils.HasKey(GENERAL_SETTINGS_KEY))
            {
                try
                {
                    object currentSetting = currentGeneralSettings;
                    JsonUtility.FromJsonOverwrite(PlayerPrefsUtils.GetString(GENERAL_SETTINGS_KEY), currentSetting);
                    currentGeneralSettings = (SettingsData.GeneralSettings)currentSetting;
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }

        private void LoadAudioSettings()
        {
            currentAudioSettings = GetDefaultAudioSettings();

            if (PlayerPrefsUtils.HasKey(AUDIO_SETTINGS_KEY))
            {
                try
                {
                    object currentSetting = currentAudioSettings;
                    JsonUtility.FromJsonOverwrite(PlayerPrefsUtils.GetString(AUDIO_SETTINGS_KEY), currentSetting);
                    currentAudioSettings = (SettingsData.AudioSettings)currentSetting;
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }

        public void LoadDefaultSettings()
        {
            autosettingsEnabled.Set(false);
            currentQualitySettings = qualitySettingsPreset.defaultPreset;
            currentGeneralSettings = GetDefaultGeneralSettings();
            currentAudioSettings = GetDefaultAudioSettings();

            ApplyQualitySettings(currentQualitySettings);
            ApplyGeneralSettings(currentGeneralSettings);
            ApplyAudioSettings(currentAudioSettings);
        }

        public void ResetAllSettings()
        {
            LoadDefaultSettings();
            SaveSettings();
            OnResetAllSettings?.Invoke();
        }

        private SettingsData.GeneralSettings GetDefaultGeneralSettings()
        {
            return new SettingsData.GeneralSettings()
            {
                mouseSensitivity = 0.6f,
                scenesLoadRadius = 4,
                avatarsLODDistance = 16,
                maxNonLODAvatars = DataStore.DataStore_AvatarsLOD.DEFAULT_MAX_AVATAR,
                voiceChatVolume = 1,
                voiceChatAllow = SettingsData.GeneralSettings.VoiceChatAllow.ALL_USERS,
                autoqualityOn = false
            };
        }

        private SettingsData.AudioSettings GetDefaultAudioSettings()
        {
            return new SettingsData.AudioSettings()
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
            lastValidAutoqualitySet.baseResolution = currentQualitySettings.baseResolution;
            lastValidAutoqualitySet.fpsCap = currentQualitySettings.fpsCap;

            if (currentQualitySettings.Equals(lastValidAutoqualitySet))
                return;

            ApplyQualitySettings(lastValidAutoqualitySet);
        }

        public void ApplyQualitySettings(SettingsData.QualitySettings settings)
        {
            if (settings.Equals(currentQualitySettings))
                return;

            currentQualitySettings = settings;
            OnQualitySettingsChanged?.Invoke(settings);
        }

        public void ApplyGeneralSettings(SettingsData.GeneralSettings settings)
        {
            if (settings.Equals(currentGeneralSettings))
                return;

            currentGeneralSettings = settings;
            OnGeneralSettingsChanged?.Invoke(settings);
            autosettingsEnabled.Set(settings.autoqualityOn);
        }

        public void ApplyAudioSettings(SettingsData.AudioSettings settings)
        {
            if (settings.Equals(currentAudioSettings))
                return;

            currentAudioSettings = settings;
            OnAudioSettingsChanged?.Invoke(settings);
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
            audioMixer.SetFloat("AllBusVolume", Utils.ToAudioMixerGroupVolume(currentAudioSettings.masterVolume));

            // Update voice chat volume, as it does not pass through the AudioMixer
            ApplyVoiceChatSettings();
        }

        public void ApplyVoiceChatSettings(float currentDataStoreVolume = 0f, float previousDataStoreVolume = 0f)
        {
            float calculatedVolume = Utils.ToVolumeCurve(DataStore.i.virtualAudioMixer.voiceChatVolume.Get() * currentAudioSettings.voiceChatVolume * currentAudioSettings.masterVolume);
            WebInterface.ApplySettings(calculatedVolume, (int)currentGeneralSettings.voiceChatAllow);
        }

        public void ApplyAvatarSFXVolume(float currentDataStoreVolume = 0f, float previousDataStoreVolume = 0f) { audioMixer.SetFloat("AvatarSFXBusVolume", Utils.ToAudioMixerGroupVolume(DataStore.i.virtualAudioMixer.avatarSFXVolume.Get() * currentAudioSettings.avatarSFXVolume)); }

        public void ApplyUISFXVolume(float currentDataStoreVolume = 0f, float previousDataStoreVolume = 0f) { audioMixer.SetFloat("UIBusVolume", Utils.ToAudioMixerGroupVolume(DataStore.i.virtualAudioMixer.uiSFXVolume.Get() * currentAudioSettings.uiSFXVolume)); }

        public void ApplyMusicVolume(float currentDataStoreVolume = 0f, float previousDataStoreVolume = 0f) { audioMixer.SetFloat("MusicBusVolume", Utils.ToAudioMixerGroupVolume(DataStore.i.virtualAudioMixer.uiSFXVolume.Get() * currentAudioSettings.musicVolume)); }

        public void SaveSettings()
        {
            PlayerPrefsUtils.SetString(GENERAL_SETTINGS_KEY, JsonUtility.ToJson(currentGeneralSettings));
            PlayerPrefsUtils.SetString(QUALITY_SETTINGS_KEY, JsonUtility.ToJson(currentQualitySettings));
            PlayerPrefsUtils.SetString(AUDIO_SETTINGS_KEY, JsonUtility.ToJson(currentAudioSettings));
            PlayerPrefsUtils.Save();
        }
    }
}

namespace DCL.SettingsData
{
    [Serializable]
    public struct GeneralSettings
    {
        public enum VoiceChatAllow { ALL_USERS, VERIFIED_ONLY, FRIENDS_ONLY }

        public float mouseSensitivity;
        public float voiceChatVolume;
        public VoiceChatAllow voiceChatAllow;
        public bool autoqualityOn;
        public float scenesLoadRadius;
        public float avatarsLODDistance;
        public float maxNonLODAvatars;

        public bool Equals(GeneralSettings settings)
        {
            return mouseSensitivity == settings.mouseSensitivity
                   && scenesLoadRadius == settings.scenesLoadRadius
                   && avatarsLODDistance == settings.avatarsLODDistance
                   && maxNonLODAvatars == settings.maxNonLODAvatars
                   && voiceChatVolume == settings.voiceChatVolume
                   && voiceChatAllow == settings.voiceChatAllow
                   && autoqualityOn == settings.autoqualityOn;
        }
    }

    [Serializable]
    public struct AudioSettings
    {
        public float masterVolume;
        public float voiceChatVolume;
        public float avatarSFXVolume;
        public float uiSFXVolume;
        public float sceneSFXVolume; // Note(Mordi): Also known as "World SFX"
        public float musicVolume;
        public bool chatSFXEnabled;

        public bool Equals(AudioSettings settings)
        {
            return masterVolume == settings.masterVolume
                   && voiceChatVolume == settings.voiceChatVolume
                   && avatarSFXVolume == settings.avatarSFXVolume
                   && uiSFXVolume == settings.uiSFXVolume
                   && sceneSFXVolume == settings.sceneSFXVolume
                   && musicVolume == settings.musicVolume
                   && chatSFXEnabled == settings.chatSFXEnabled;
        }
    }
}