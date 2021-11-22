using System;
using UnityEngine;

namespace DCL.SettingsCommon
{
    public class PlayerPrefsQualitySettingsRepository : ISettingsRepository<QualitySettings>
    {
        private readonly IPlayerPrefsSettingsByKey settingsByKey;
        private readonly QualitySettings defaultSettings;
        private QualitySettings currentSettings;
        
        public event Action<QualitySettings> OnChanged;

        public PlayerPrefsQualitySettingsRepository(
            IPlayerPrefsSettingsByKey settingsByKey,
            QualitySettings defaultSettings)
        {
            this.settingsByKey = settingsByKey;
            this.defaultSettings = defaultSettings;
            currentSettings = Load();
        }

        public QualitySettings Data => currentSettings;

        public void Apply(QualitySettings settings)
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
            settingsByKey.SetString("displayName", currentSettings.displayName);
            settingsByKey.SetBool("bloom", currentSettings.bloom);
            settingsByKey.SetBool("colorGrading", currentSettings.colorGrading);
            settingsByKey.SetBool("fpsCap", currentSettings.fpsCap);
            settingsByKey.SetBool("softShadows", currentSettings.softShadows);
            settingsByKey.SetBool("enableDetailObjectCulling", currentSettings.enableDetailObjectCulling);
            settingsByKey.SetBool("shadows", currentSettings.shadows);
            settingsByKey.SetFloat("renderScale", currentSettings.renderScale);
            settingsByKey.SetFloat("shadowDistance", currentSettings.shadowDistance);
            settingsByKey.SetFloat("cameraDrawDistance", currentSettings.cameraDrawDistance);
            settingsByKey.SetFloat("detailObjectCullingLimit", currentSettings.detailObjectCullingLimit);
            settingsByKey.SetEnum("antiAliasing", currentSettings.antiAliasing);
            settingsByKey.SetEnum("baseResolution", currentSettings.baseResolution);
            settingsByKey.SetEnum("shadowResolution", currentSettings.shadowResolution);
            settingsByKey.SetEnum("ssaoQuality", currentSettings.ssaoQuality);
            settingsByKey.SetInt("maxHQAvatars", currentSettings.maxHQAvatars);
        }

        public bool HasAnyData() => !Data.Equals(defaultSettings);

        private QualitySettings Load()
        {
            var settings = defaultSettings;
            
            try
            {
                settings.displayName = settingsByKey.GetString("displayName", defaultSettings.displayName);
                settings.bloom = settingsByKey.GetBool("bloom", defaultSettings.bloom);
                settings.colorGrading = settingsByKey.GetBool("colorGrading", defaultSettings.colorGrading);
                settings.fpsCap = settingsByKey.GetBool("fpsCap", defaultSettings.fpsCap);
                settings.softShadows = settingsByKey.GetBool("softShadows", defaultSettings.softShadows);
                settings.enableDetailObjectCulling = settingsByKey.GetBool("enableDetailObjectCulling", defaultSettings.enableDetailObjectCulling);
                settings.shadows = settingsByKey.GetBool("shadows", defaultSettings.shadows);
                settings.renderScale = settingsByKey.GetFloat("renderScale", defaultSettings.renderScale);
                settings.shadowDistance = settingsByKey.GetFloat("shadowDistance", defaultSettings.shadowDistance);
                settings.cameraDrawDistance = settingsByKey.GetFloat("cameraDrawDistance", defaultSettings.cameraDrawDistance);
                settings.detailObjectCullingLimit = settingsByKey.GetFloat("detailObjectCullingLimit", defaultSettings.detailObjectCullingLimit);
                settings.antiAliasing = settingsByKey.GetEnum("antiAliasing", defaultSettings.antiAliasing);
                settings.baseResolution = settingsByKey.GetEnum("baseResolution", defaultSettings.baseResolution);
                settings.shadowResolution = settingsByKey.GetEnum("shadowResolution", defaultSettings.shadowResolution);
                settings.ssaoQuality = settingsByKey.GetEnum("ssaoQuality", defaultSettings.ssaoQuality);
                settings.maxHQAvatars = settingsByKey.GetInt("maxHQAvatars", defaultSettings.maxHQAvatars);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return settings;
        }
    }
}