using UnityEngine;
using System;

namespace DCL
{
    public class Settings : DCL.Singleton<Settings>
    {
        const string QUALITY_SETTINGS_KEY = "Settings.Quality";
        const string GENERAL_SETTINGS_KEY = "Settings.General";

        public event Action<SettingsHUD.QualitySettings> OnQualitySettingsChanged;
        public event Action<SettingsHUD.GeneralSettings> OnGeneralSettingsChanged;

        public SettingsHUD.QualitySettings qualitySettings { get { return currentQualitySettings; } }
        public SettingsHUD.QualitySettingsData qualitySettingsPresets { get { return qualitySettingsPreset; } }
        public SettingsHUD.GeneralSettings generalSettings { get { return currentGeneralSettings; } }

        private static SettingsHUD.QualitySettingsData qualitySettingsPreset = null;

        private SettingsHUD.QualitySettings currentQualitySettings;
        private SettingsHUD.GeneralSettings currentGeneralSettings;

        public Settings()
        {
            if (qualitySettingsPreset == null)
            {
                qualitySettingsPreset = Resources.Load<SettingsHUD.QualitySettingsData>("ScriptableObjects/QualitySettingsData");
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
                    currentQualitySettings = JsonUtility.FromJson<SettingsHUD.QualitySettings>(PlayerPrefs.GetString(QUALITY_SETTINGS_KEY));
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
            bool isGeneralSettingsSet = false;
            if (PlayerPrefs.HasKey(GENERAL_SETTINGS_KEY))
            {
                try
                {
                    currentGeneralSettings = JsonUtility.FromJson<SettingsHUD.GeneralSettings>(PlayerPrefs.GetString(GENERAL_SETTINGS_KEY));
                    isGeneralSettingsSet = true;
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            }
            if (!isGeneralSettingsSet)
            {
                currentGeneralSettings = new SettingsHUD.GeneralSettings()
                {
                    sfxVolume = 1,
                    mouseSensitivity = 0.2f
                };
            }
        }

        public void ApplyQualitySettingsPreset(int index)
        {
            if (index >= 0 && index < qualitySettingsPreset.Length)
            {
                ApplyQualitySettings(qualitySettingsPreset[index]);
            }
        }

        public void ApplyQualitySettings(SettingsHUD.QualitySettings settings)
        {
            currentQualitySettings = settings;
            OnQualitySettingsChanged?.Invoke(settings);
        }

        public void ApplyGeneralSettings(SettingsHUD.GeneralSettings settings)
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

namespace DCL.SettingsHUD
{
    [Serializable]
    public struct GeneralSettings
    {
        public float sfxVolume;
        public float mouseSensitivity;

        public bool Equals(GeneralSettings settings)
        {
            return sfxVolume == settings.sfxVolume
                && mouseSensitivity == settings.mouseSensitivity;
        }
    }
}