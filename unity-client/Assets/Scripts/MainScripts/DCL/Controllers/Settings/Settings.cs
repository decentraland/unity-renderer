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

        private SettingsData.QualitySettings currentQualitySettings;
        private SettingsData.GeneralSettings currentGeneralSettings;

        public Settings()
        {
            if (qualitySettingsPreset == null)
            {
                qualitySettingsPreset = Resources.Load<SettingsData.QualitySettingsData>("ScriptableObjects/QualitySettingsData");
            }
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
            currentGeneralSettings = new SettingsData.GeneralSettings()
            {
                sfxVolume = 1,
                mouseSensitivity = 0.2f,
                voiceChatVolume = 1,
                voiceChatAllow = SettingsData.GeneralSettings.VoiceChatAllow.ALL_USERS
            };

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

        public void ApplyQualitySettingsPreset(int index)
        {
            if (index >= 0 && index < qualitySettingsPreset.Length)
            {
                ApplyQualitySettings(qualitySettingsPreset[index]);
            }
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

        public bool Equals(GeneralSettings settings)
        {
            return sfxVolume == settings.sfxVolume
                && mouseSensitivity == settings.mouseSensitivity
                && voiceChatVolume == settings.voiceChatVolume
                && voiceChatAllow == settings.voiceChatAllow;
        }
    }
}