using NSubstitute;
using NUnit.Framework;

namespace DCL.SettingsCommon.Tests
{
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
                autoqualityOn = true,
                namesOpacity = 0.75f,
                mouseSensitivity = 0.1f,
                profanityChatFiltering = true,
                scenesLoadRadius = 4
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
                autoqualityOn = true,
                namesOpacity = 0.75f,
                mouseSensitivity = 0.1f,
            };
            var fallbackEditedSettings = new GeneralSettings
            {
                autoqualityOn = true,
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
                autoqualityOn = true,
                namesOpacity = 0.75f,
                voiceChatAllow = GeneralSettings.VoiceChatAllow.VERIFIED_ONLY
            };
            var settingsByKey = GivenDataStoredInPrefs(latestEditedSettings);
            GivenMissingBoolAttribute(settingsByKey, "profanityChatFiltering");
            GivenMissingFloatAttribute(settingsByKey, "scenesLoadRadius");
            var defaultSettings = GetDefaultSettings();
            var latestRepository = new PlayerPrefsGeneralSettingsRepository(
                settingsByKey, defaultSettings);
            var fallbackRepository = GivenSettingsRepositoryWithNoData();
            var proxyRepository = new ProxySettingsRepository<GeneralSettings>(latestRepository,
                fallbackRepository);
            
            var settings = proxyRepository.Data;
            
            Assert.AreEqual(latestEditedSettings.autoqualityOn, settings.autoqualityOn);
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
                autoqualityOn = false,
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
            prefsByKey.GetBool("autoqualityOn", Arg.Any<bool>()).Returns(settings.autoqualityOn);
            prefsByKey.GetBool("profanityChatFiltering", Arg.Any<bool>()).Returns(settings.profanityChatFiltering);
            prefsByKey.GetFloat("mouseSensitivity", Arg.Any<float>()).Returns(settings.mouseSensitivity);
            prefsByKey.GetFloat("namesOpacity", Arg.Any<float>()).Returns(settings.namesOpacity);
            prefsByKey.GetFloat("scenesLoadRadius", Arg.Any<float>()).Returns(settings.scenesLoadRadius);
            prefsByKey.GetFloat("voiceChatVolume", Arg.Any<float>()).Returns(settings.voiceChatVolume);
            prefsByKey.GetFloat("avatarsLODDistance", Arg.Any<float>()).Returns(settings.avatarsLODDistance);
            prefsByKey.GetFloat("maxNonLODAvatars", Arg.Any<float>()).Returns(settings.maxNonLODAvatars);
            prefsByKey.GetEnum("voiceChatAllow", Arg.Any<GeneralSettings.VoiceChatAllow>())
                .Returns(settings.voiceChatAllow);
            return prefsByKey;
        }

        private IPlayerPrefsSettingsByKey GivenNoDataStoredInPrefs()
        {
            var prefsBykey = Substitute.For<IPlayerPrefsSettingsByKey>();
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