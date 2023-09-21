using UnityEngine;
using UnityEngine.Audio;

namespace DCL.SettingsCommon
{
    public class DefaultSettingsFactory : ISettingsFactory
    {
        private const string QUALITY_SETTINGS_KEY = "Settings.Quality";
        private const string GENERAL_SETTINGS_KEY = "Settings.General";
        private const string AUDIO_SETTINGS_KEY = "Settings.Audio";

        private string graphicsQualitySettingsPresetPath = "ScriptableObjects/QualitySettingsData";
        private string audioMixerPath = "AudioMixer";

        private GeneralSettings defaultGeneralSettings = new GeneralSettings
        {
            leftMouseButtonCursorLock = true,
            mouseSensitivity = 0.6f,
            invertYAxis = false,

            scenesLoadRadius = 3,
            avatarsLODDistance = 16,
            maxNonLODAvatars = DataStore_AvatarsLOD.DEFAULT_MAX_AVATAR,
            voiceChatVolume = 1,
            voiceChatAllow = GeneralSettings.VoiceChatAllow.ALL_USERS,
            namesOpacity = 1f,
            profanityChatFiltering = true,
            nightMode = false,
            hideUI = false,
            showAvatarNames = true,
            dynamicProceduralSkybox = true,
            skyboxTime = 0.0f,
            firstPersonCameraFOV = 60,
            adultContent = false,
        };

        private readonly AudioSettings defaultAudioSettings = new ()
        {
            masterVolume = 1f,
            voiceChatVolume = 1f,
            avatarSFXVolume = 1f,
            uiSFXVolume = 1f,
            sceneSFXVolume = 1f,
            musicVolume = 1f,
            chatNotificationType = AudioSettings.ChatNotificationType.All,
        };

        public DefaultSettingsFactory WithDefaultGeneralSettings(GeneralSettings settings)
        {
            defaultGeneralSettings = settings;
            return this;
        }

        public DefaultSettingsFactory WithGraphicsQualitySettingsPresetPath(string path)
        {
            graphicsQualitySettingsPresetPath = path;
            return this;
        }

        public Settings Build()
        {
            var graphicsQualitySettingsPreset = Resources.Load<QualitySettingsData>(graphicsQualitySettingsPresetPath);
            var audioMixer = Resources.Load<AudioMixer>(audioMixerPath);

            return new Settings(graphicsQualitySettingsPreset,
                audioMixer,
                CreateGraphicsQualityRepository(graphicsQualitySettingsPreset),
                CreateGeneralSettingsRepository(),
                CreateAudioSettingsRepository());
        }

        private ProxySettingsRepository<AudioSettings> CreateAudioSettingsRepository()
        {
            return new ProxySettingsRepository<AudioSettings>(
                new PlayerPrefsAudioSettingsRepository(
                    new PlayerPrefsSettingsByKey(AUDIO_SETTINGS_KEY),
                    defaultAudioSettings),
                new SettingsModule<AudioSettings>(
                    AUDIO_SETTINGS_KEY,
                    defaultAudioSettings));
        }

        private ProxySettingsRepository<GeneralSettings> CreateGeneralSettingsRepository()
        {
            return new ProxySettingsRepository<GeneralSettings>(
                new PlayerPrefsGeneralSettingsRepository(
                    new PlayerPrefsSettingsByKey(GENERAL_SETTINGS_KEY),
                    defaultGeneralSettings),
                new SettingsModule<GeneralSettings>(
                    GENERAL_SETTINGS_KEY,
                    defaultGeneralSettings));
        }

        private ProxySettingsRepository<QualitySettings> CreateGraphicsQualityRepository(QualitySettingsData qualitySettingsPreset)
        {
            return new ProxySettingsRepository<QualitySettings>(
                new PlayerPrefsQualitySettingsRepository(
                    new PlayerPrefsSettingsByKey(QUALITY_SETTINGS_KEY),
                    qualitySettingsPreset.defaultPreset),
                new SettingsModule<QualitySettings>(
                    QUALITY_SETTINGS_KEY,
                    qualitySettingsPreset.defaultPreset));
        }
    }
}
