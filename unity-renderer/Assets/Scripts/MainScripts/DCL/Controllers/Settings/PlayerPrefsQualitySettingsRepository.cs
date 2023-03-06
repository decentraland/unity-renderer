using System;
using UnityEngine;

namespace DCL.SettingsCommon
{
    public class PlayerPrefsQualitySettingsRepository : ISettingsRepository<QualitySettings>
    {
        private const string DISPLAY_NAME = "displayName";
        private const string BLOOM = "bloom";
        private const string FPS_CAP = "fpsCap";
        private const string SOFT_SHADOWS = "softShadows";
        private const string ENABLE_DETAIL_OBJECT_CULLING = "enableDetailObjectCulling";
        private const string SHADOWS = "shadows";
        private const string RENDER_SCALE = "renderScale";
        private const string SHADOW_DISTANCE = "shadowDistance";
        private const string CAMERA_DRAW_DISTANCE = "cameraDrawDistance";
        private const string DETAIL_OBJECT_CULLING_LIMIT = "detailObjectCullingLimit";
        private const string ANTI_ALIASING = "antiAliasing";
        private const string BASE_RESOLUTION = "baseResolution";
        private const string SHADOW_RESOLUTION = "shadowResolution";
        private const string SSAO_QUALITY = "ssaoQuality";
        private const string MAX_HQ_AVATARS = "maxHQAvatars";
        private const string REFLECTION_RESOLUTION = "reflectionResolution";
        private const string SHADER_QUALITY = "shaderQuality";

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
            if (currentSettings.Equals(settings))
                return;
            currentSettings = settings;
            OnChanged?.Invoke(currentSettings);
        }

        public void Reset() { Apply(defaultSettings); }

        public void Save()
        {
            settingsByKey.SetString(DISPLAY_NAME, currentSettings.displayName);
            settingsByKey.SetBool(BLOOM, currentSettings.bloom);
            settingsByKey.SetBool(FPS_CAP, currentSettings.fpsCap);
            settingsByKey.SetBool(SOFT_SHADOWS, currentSettings.softShadows);
            settingsByKey.SetBool(ENABLE_DETAIL_OBJECT_CULLING, currentSettings.enableDetailObjectCulling);
            settingsByKey.SetBool(SHADOWS, currentSettings.shadows);
            settingsByKey.SetFloat(RENDER_SCALE, currentSettings.renderScale);
            settingsByKey.SetFloat(SHADOW_DISTANCE, currentSettings.shadowDistance);
            settingsByKey.SetFloat(CAMERA_DRAW_DISTANCE, currentSettings.cameraDrawDistance);
            settingsByKey.SetFloat(DETAIL_OBJECT_CULLING_LIMIT, currentSettings.detailObjectCullingLimit);
            settingsByKey.SetEnum(ANTI_ALIASING, currentSettings.antiAliasing);
            settingsByKey.SetEnum(BASE_RESOLUTION, currentSettings.baseResolution);
            settingsByKey.SetEnum(SHADOW_RESOLUTION, currentSettings.shadowResolution);
            settingsByKey.SetEnum(SSAO_QUALITY, currentSettings.ssaoQuality);
            settingsByKey.SetInt(MAX_HQ_AVATARS, currentSettings.maxHQAvatars);
            settingsByKey.SetEnum(REFLECTION_RESOLUTION, currentSettings.reflectionResolution);
            settingsByKey.SetEnum(SHADER_QUALITY, currentSettings.shaderQuality);
        }

        public bool HasAnyData() => !Data.Equals(defaultSettings);

        private QualitySettings Load()
        {
            var settings = defaultSettings;

            try
            {
                settings.displayName = settingsByKey.GetString(DISPLAY_NAME, defaultSettings.displayName);
                settings.bloom = settingsByKey.GetBool(BLOOM, defaultSettings.bloom);
                settings.fpsCap = settingsByKey.GetBool(FPS_CAP, defaultSettings.fpsCap);
                settings.softShadows = settingsByKey.GetBool(SOFT_SHADOWS, defaultSettings.softShadows);
                settings.enableDetailObjectCulling = settingsByKey.GetBool(ENABLE_DETAIL_OBJECT_CULLING, defaultSettings.enableDetailObjectCulling);
                settings.shadows = settingsByKey.GetBool(SHADOWS, defaultSettings.shadows);
                settings.renderScale = settingsByKey.GetFloat(RENDER_SCALE, defaultSettings.renderScale);
                settings.shadowDistance = settingsByKey.GetFloat(SHADOW_DISTANCE, defaultSettings.shadowDistance);
                settings.cameraDrawDistance = settingsByKey.GetFloat(CAMERA_DRAW_DISTANCE, defaultSettings.cameraDrawDistance);
                settings.detailObjectCullingLimit = settingsByKey.GetFloat(DETAIL_OBJECT_CULLING_LIMIT, defaultSettings.detailObjectCullingLimit);
                settings.antiAliasing = settingsByKey.GetEnum(ANTI_ALIASING, defaultSettings.antiAliasing);
                settings.baseResolution = settingsByKey.GetEnum(BASE_RESOLUTION, defaultSettings.baseResolution);
                settings.shadowResolution = settingsByKey.GetEnum(SHADOW_RESOLUTION, defaultSettings.shadowResolution);
                settings.ssaoQuality = settingsByKey.GetEnum(SSAO_QUALITY, defaultSettings.ssaoQuality);
                settings.maxHQAvatars = settingsByKey.GetInt(MAX_HQ_AVATARS, defaultSettings.maxHQAvatars);
                settings.reflectionResolution = settingsByKey.GetEnum(REFLECTION_RESOLUTION, defaultSettings.reflectionResolution);
                settings.shaderQuality = settingsByKey.GetEnum(SHADER_QUALITY, defaultSettings.shaderQuality);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return settings;
        }
    }
}
