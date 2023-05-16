using NSubstitute;
using NUnit.Framework;

namespace DCL.SettingsCommon.Tests
{
    [Category("EditModeCI")]
    public class ProxySettingsRepositoryShould
    {
        [Test]
        public void ReturnDefaultDataWhenRepositoriesHasNoData()
        {
            var latestRepository = GivenSettingsRepositoryWithNoData();
            var fallbackRepository = GivenSettingsRepositoryWithNoData();
            var proxyRepository = new ProxySettingsRepository<GeneralSettings>(latestRepository,
                fallbackRepository);

            var settings = proxyRepository.Data;
            var latestRepositorySettings = latestRepository.Data;

            Assert.AreEqual(GetDefaultSettings(), settings);
            ThenDataWasMigratedToLatestRepository(settings, latestRepositorySettings);
        }

        [Test]
        public void ReturnFallbackDataWhenLatestRepositoryHasNoData()
        {
            var latestRepository = GivenSettingsRepositoryWithNoData();
            var editedSettings = new GeneralSettings
            {
                namesOpacity = 0.75f,
                mouseSensitivity = 0.1f,
                profanityChatFiltering = true,
                scenesLoadRadius = 3
            };
            var fallbackRepository = GivenSettingsRepositoryWithDataStored(editedSettings);
            var proxyRepository = new ProxySettingsRepository<GeneralSettings>(latestRepository,
                fallbackRepository);

            var settings = proxyRepository.Data;
            var latestRepositorySettings = latestRepository.Data;

            Assert.AreEqual(editedSettings, settings);
            ThenDataWasMigratedToLatestRepository(settings, latestRepositorySettings);
        }

        [Test]
        public void ReturnLatestRepositoryDataWhenHasAnyDataStored()
        {
            var latestEditedSettings = new GeneralSettings
            {
                namesOpacity = 0.75f,
                mouseSensitivity = 0.1f,
            };
            var fallbackEditedSettings = new GeneralSettings
            {
                namesOpacity = 0.23f,
                mouseSensitivity = 0.7f,
                profanityChatFiltering = false,
                scenesLoadRadius = 1,
                avatarsLODDistance = 0.6f,
                voiceChatAllow = GeneralSettings.VoiceChatAllow.FRIENDS_ONLY
            };
            var latestRepository = GivenSettingsRepositoryWithDataStored(latestEditedSettings);
            var fallbackRepository = GivenSettingsRepositoryWithDataStored(fallbackEditedSettings);
            var proxyRepository = new ProxySettingsRepository<GeneralSettings>(latestRepository,
                fallbackRepository);

            var settings = proxyRepository.Data;

            Assert.AreEqual(latestEditedSettings, settings);
        }

        [Test]
        public void DefaultMissingAttributes()
        {
            var latestEditedSettings = new GeneralSettings
            {
                namesOpacity = 0.75f,
                voiceChatAllow = GeneralSettings.VoiceChatAllow.VERIFIED_ONLY
            };
            var settingsByKey = GivenDataStoredInPrefs(latestEditedSettings);

            GivenMissingBoolAttribute(settingsByKey, PlayerPrefsGeneralSettingsRepository.PROFANITY_CHAT_FILTERING);
            GivenMissingFloatAttribute(settingsByKey, PlayerPrefsGeneralSettingsRepository.SCENES_LOAD_RADIUS);
            var defaultSettings = GetDefaultSettings();
            var latestRepository = new PlayerPrefsGeneralSettingsRepository(
                settingsByKey, defaultSettings);
            var fallbackRepository = GivenSettingsRepositoryWithNoData();
            var proxyRepository = new ProxySettingsRepository<GeneralSettings>(latestRepository,
                fallbackRepository);

            var settings = proxyRepository.Data;

            Assert.AreEqual(latestEditedSettings.namesOpacity, settings.namesOpacity);
            Assert.AreEqual(latestEditedSettings.voiceChatAllow, settings.voiceChatAllow);
            Assert.AreEqual(defaultSettings.profanityChatFiltering, settings.profanityChatFiltering);
            Assert.AreEqual(defaultSettings.scenesLoadRadius, settings.scenesLoadRadius);
        }

        private void GivenMissingFloatAttribute(IPlayerPrefsSettingsByKey settingsByKey, string fieldName)
        {
            settingsByKey.GetFloat(fieldName, Arg.Any<float>())
                .Returns(call => call[1]);
        }

        private void GivenMissingBoolAttribute(IPlayerPrefsSettingsByKey settingsByKey, string fieldName)
        {
            settingsByKey.GetBool(fieldName, Arg.Any<bool>())
                .Returns(call => call[1]);
        }

        private GeneralSettings GetDefaultSettings()
        {
            return new GeneralSettings
            {
                mouseSensitivity = 0.6f,
                scenesLoadRadius = 4,
                avatarsLODDistance = 16,
                maxNonLODAvatars = 1,
                voiceChatVolume = 1,
                voiceChatAllow = GeneralSettings.VoiceChatAllow.ALL_USERS,
                namesOpacity = 0.5f,
                profanityChatFiltering = true
            };
        }

        private PlayerPrefsGeneralSettingsRepository GivenSettingsRepositoryWithDataStored(GeneralSettings settings)
        {
            return new PlayerPrefsGeneralSettingsRepository(GivenDataStoredInPrefs(settings),
                GetDefaultSettings());
        }

        private PlayerPrefsGeneralSettingsRepository GivenSettingsRepositoryWithNoData()
        {
            var latestRepository = new PlayerPrefsGeneralSettingsRepository(GivenNoDataStoredInPrefs(),
                GetDefaultSettings());
            return latestRepository;
        }

        private IPlayerPrefsSettingsByKey GivenDataStoredInPrefs(GeneralSettings settings)
        {
            var prefsByKey = Substitute.For<IPlayerPrefsSettingsByKey>();
            prefsByKey.GetBool(PlayerPrefsGeneralSettingsRepository.PROFANITY_CHAT_FILTERING,
                Arg.Any<bool>()).Returns(settings.profanityChatFiltering);
            prefsByKey.GetFloat(PlayerPrefsGeneralSettingsRepository.MOUSE_SENSITIVITY,
                Arg.Any<float>()).Returns(settings.mouseSensitivity);
            prefsByKey.GetFloat(PlayerPrefsGeneralSettingsRepository.NAMES_OPACITY,
                Arg.Any<float>()).Returns(settings.namesOpacity);
            prefsByKey.GetFloat(PlayerPrefsGeneralSettingsRepository.SCENES_LOAD_RADIUS,
                Arg.Any<float>()).Returns(settings.scenesLoadRadius);
            prefsByKey.GetFloat(PlayerPrefsGeneralSettingsRepository.VOICE_CHAT_VOLUME,
                Arg.Any<float>()).Returns(settings.voiceChatVolume);
            prefsByKey.GetFloat(PlayerPrefsGeneralSettingsRepository.AVATARS_LOD_DISTANCE,
                Arg.Any<float>()).Returns(settings.avatarsLODDistance);
            prefsByKey.GetFloat(PlayerPrefsGeneralSettingsRepository.MAX_NON_LOAD_AVATARS,
                Arg.Any<float>()).Returns(settings.maxNonLODAvatars);
            prefsByKey.GetEnum(PlayerPrefsGeneralSettingsRepository.VOICE_CHAT_ALLOW,
                    Arg.Any<GeneralSettings.VoiceChatAllow>()).Returns(settings.voiceChatAllow);
            return prefsByKey;
        }

        private IPlayerPrefsSettingsByKey GivenNoDataStoredInPrefs()
        {
            var prefsBykey = Substitute.For<IPlayerPrefsSettingsByKey>();
            prefsBykey.GetString(Arg.Any<string>(), Arg.Any<string>()).Returns(info => info.Args()[1]);
            prefsBykey.GetBool(Arg.Any<string>(), Arg.Any<bool>()).Returns(info => info.Args()[1]);
            prefsBykey.GetFloat(Arg.Any<string>(), Arg.Any<float>()).Returns(info => info.Args()[1]);
            prefsBykey.GetInt(Arg.Any<string>(), Arg.Any<int>()).Returns(info => info.Args()[1]);
            prefsBykey.GetEnum(Arg.Any<string>(), Arg.Any<GeneralSettings.VoiceChatAllow>())
                .Returns(info => info.Args()[1]);
            return prefsBykey;
        }

        private void ThenDataWasMigratedToLatestRepository(GeneralSettings settings,
            GeneralSettings latestRepositorySettings)
        {
            Assert.AreEqual(settings, latestRepositorySettings);
        }
    }
}
