using System;
using UnityEngine;

namespace DCL.SettingsCommon
{
    public class PlayerPrefsGeneralSettingsRepository : ISettingsRepository<GeneralSettings>
    {
        public const string PROFANITY_CHAT_FILTERING = "profanityChatFiltering";
        public const string MOUSE_SENSITIVITY = "mouseSensitivity";
        public const string NAMES_OPACITY = "namesOpacity";
        public const string SCENES_LOAD_RADIUS = "scenesLoadRadius";
        public const string VOICE_CHAT_VOLUME = "voiceChatVolume";
        public const string AVATARS_LOD_DISTANCE = "avatarsLODDistance";
        public const string MAX_NON_LOAD_AVATARS = "maxNonLODAvatars";
        public const string VOICE_CHAT_ALLOW = "voiceChatAllow";
        public const string PROCEDURAL_SKYBOX_MODE = "ProceduralSkyboxMode";
        public const string INVERT_Y_AXIS = "InvertYAxis";
        public const string SKYBOX_TIME = "skyboxTime";
        public const string FIRST_PERSON_CAMERA_FOV = "firstPersonCameraFOV";

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
            if (currentSettings.Equals(settings))
                return;
            currentSettings = settings;
            OnChanged?.Invoke(currentSettings);
        }

        public void Reset() { Apply(defaultSettings); }

        public void Save()
        {
            settingsByKey.SetBool(PROFANITY_CHAT_FILTERING, currentSettings.profanityChatFiltering);
            settingsByKey.SetFloat(MOUSE_SENSITIVITY, currentSettings.mouseSensitivity);
            settingsByKey.SetFloat(NAMES_OPACITY, currentSettings.namesOpacity);
            settingsByKey.SetFloat(SCENES_LOAD_RADIUS, currentSettings.scenesLoadRadius);
            settingsByKey.SetFloat(VOICE_CHAT_VOLUME, currentSettings.voiceChatVolume);
            settingsByKey.SetFloat(AVATARS_LOD_DISTANCE, currentSettings.avatarsLODDistance);
            settingsByKey.SetFloat(MAX_NON_LOAD_AVATARS, currentSettings.maxNonLODAvatars);
            settingsByKey.SetEnum(VOICE_CHAT_ALLOW, currentSettings.voiceChatAllow);
            settingsByKey.SetBool(PROCEDURAL_SKYBOX_MODE, currentSettings.dynamicProceduralSkybox);
            settingsByKey.SetBool(INVERT_Y_AXIS, currentSettings.invertYAxis);
            settingsByKey.SetFloat(SKYBOX_TIME, currentSettings.skyboxTime);
            settingsByKey.SetFloat(FIRST_PERSON_CAMERA_FOV, currentSettings.firstPersonCameraFOV);
        }

        public bool HasAnyData() => !Data.Equals(defaultSettings);

        private GeneralSettings Load()
        {
            var settings = defaultSettings;

            try
            {
                settings.profanityChatFiltering = settingsByKey.GetBool(PROFANITY_CHAT_FILTERING,
                    defaultSettings.profanityChatFiltering);
                settings.mouseSensitivity = settingsByKey.GetFloat(MOUSE_SENSITIVITY, defaultSettings.mouseSensitivity);
                settings.namesOpacity = settingsByKey.GetFloat(NAMES_OPACITY, defaultSettings.namesOpacity);
                settings.scenesLoadRadius = settingsByKey.GetFloat(SCENES_LOAD_RADIUS, defaultSettings.scenesLoadRadius);
                settings.voiceChatVolume = settingsByKey.GetFloat(VOICE_CHAT_VOLUME, defaultSettings.voiceChatVolume);
                settings.avatarsLODDistance = settingsByKey.GetFloat(AVATARS_LOD_DISTANCE, defaultSettings.avatarsLODDistance);
                settings.maxNonLODAvatars = settingsByKey.GetFloat(MAX_NON_LOAD_AVATARS, defaultSettings.maxNonLODAvatars);
                settings.voiceChatAllow = settingsByKey.GetEnum(VOICE_CHAT_ALLOW, defaultSettings.voiceChatAllow);
                settings.dynamicProceduralSkybox = settingsByKey.GetBool(PROCEDURAL_SKYBOX_MODE, defaultSettings.dynamicProceduralSkybox);
                settings.invertYAxis = settingsByKey.GetBool(INVERT_Y_AXIS, defaultSettings.invertYAxis);
                settings.skyboxTime = settingsByKey.GetFloat(SKYBOX_TIME, defaultSettings.skyboxTime);
                settings.firstPersonCameraFOV = settingsByKey.GetFloat(FIRST_PERSON_CAMERA_FOV, defaultSettings.firstPersonCameraFOV);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return settings;
        }
    }
}