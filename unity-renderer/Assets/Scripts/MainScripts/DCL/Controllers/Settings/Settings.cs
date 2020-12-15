using UnityEngine;
using System;

namespace DCL
{
    public class Settings : DCL.Singleton<Settings>
    {
        const string QUALITY_SETTINGS_KEY = "Settings.Quality";
        const string GENERAL_SETTINGS_KEY = "Settings.General";

        public event Action<SettingsData.QualitySettings> OnQualitySettingsChanged;
        public event Action<SettingsData.GeneralSettings> OnGeneralSettingsChanged;

        public SettingsData.QualitySettings qualitySettings { get { return currentQualitySettings; } }
        public SettingsData.QualitySettingsData qualitySettingsPresets { get { return qualitySettingsPreset; } }
        public SettingsData.GeneralSettings generalSettings { get { return currentGeneralSettings; } }

        private static SettingsData.QualitySettingsData qualitySettingsPreset = null;

        public SettingsData.QualitySettingsData autoqualitySettings = null;
        public SettingsData.QualitySettings lastValidAutoqualitySet;

        private readonly BooleanVariable autosettingsEnabled = null;

        public SettingsData.QualitySettings currentQualitySettings { private set; get; }
        private SettingsData.GeneralSettings currentGeneralSettings;

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
        }

        private void LoadQualitySettings()
        {
            bool isQualitySettingsSet = false;
            if (PlayerPrefs.HasKey(QUALITY_SETTINGS_KEY))
            {
                try
                {
                    currentQualitySettings = JsonUtility.FromJson<SettingsData.QualitySettings>(PlayerPrefs.GetString(QUALITY_SETTINGS_KEY));
                    isQualitySettingsSet = true;
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
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

            if (PlayerPrefs.HasKey(GENERAL_SETTINGS_KEY))
            {
                try
                {
                    object currentSetting = currentGeneralSettings;
                    JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(GENERAL_SETTINGS_KEY), currentSetting);
                    currentGeneralSettings = (SettingsData.GeneralSettings)currentSetting;
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            }
        }

        public void LoadDefaultSettings()
        {
            autosettingsEnabled.Set(false);
            currentQualitySettings = qualitySettingsPreset.defaultPreset;
            currentGeneralSettings = GetDefaultGeneralSettings();

            ApplyQualitySettings(currentQualitySettings);
            ApplyGeneralSettings(currentGeneralSettings);
        }

        private SettingsData.GeneralSettings GetDefaultGeneralSettings()
        {
            return new SettingsData.GeneralSettings()
            {
                sfxVolume = 1,
                mouseSensitivity = 0.6f,
                voiceChatVolume = 1,
                voiceChatAllow = SettingsData.GeneralSettings.VoiceChatAllow.ALL_USERS,
                autoqualityOn = false
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
            currentQualitySettings = settings;
            OnQualitySettingsChanged?.Invoke(settings);
        }

        public void ApplyGeneralSettings(SettingsData.GeneralSettings settings)
        {
            currentGeneralSettings = settings;
            OnGeneralSettingsChanged?.Invoke(settings);
            autosettingsEnabled.Set(settings.autoqualityOn);
        }

        public void SaveSettings()
        {
            PlayerPrefs.SetString(GENERAL_SETTINGS_KEY, JsonUtility.ToJson(currentGeneralSettings));
            PlayerPrefs.SetString(QUALITY_SETTINGS_KEY, JsonUtility.ToJson(currentQualitySettings));
            PlayerPrefs.Save();
        }
    }
}

namespace DCL.SettingsData
{
    [Serializable]
    public struct GeneralSettings
    {
        public enum VoiceChatAllow { ALL_USERS, VERIFIED_ONLY, FRIENDS_ONLY }

        public float sfxVolume;
        public float mouseSensitivity;
        public float voiceChatVolume;
        public VoiceChatAllow voiceChatAllow;
        public bool autoqualityOn;

        public bool Equals(GeneralSettings settings)
        {
            return sfxVolume == settings.sfxVolume
                && mouseSensitivity == settings.mouseSensitivity
                && voiceChatVolume == settings.voiceChatVolume
                && voiceChatAllow == settings.voiceChatAllow
                && autoqualityOn == settings.autoqualityOn;
        }
    }
}