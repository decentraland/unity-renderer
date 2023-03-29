using MainScripts.DCL.Controllers.SettingsDesktop;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace DCL.SettingsCommon
{
    public class PlayerPrefsDesktopDisplaySettingsRepositoryShould
    {
        [Test]
        public void GetDataWhenIsAnythingStored()
        {
            var storedSettings = new DisplaySettings
            {
                vSync = true,
                windowMode = WindowMode.Borderless,
                resolutionSizeIndex = 3,
                fpsCapIndex = 90
            };

            var settingsByKey = GivenStoredSettings(storedSettings);

            var repository = new PlayerPrefsDesktopDisplaySettingsRepository(settingsByKey,
                GetDefaultDisplaySettings());

            var settings = WhenGetSettings(repository);

            Assert.AreEqual(storedSettings, settings);
        }

        [Test][Category("ToFix")]
        public void GetDefaultSettingsWhenNothingIsStored()
        {
            var settingsByKey = GivenNoStoredSettings();
            var defaultSettings = GetDefaultDisplaySettings();

            var repository = new PlayerPrefsDesktopDisplaySettingsRepository(settingsByKey,
                defaultSettings);

            var settings = WhenGetSettings(repository);

            Assert.AreEqual(defaultSettings, settings);
        }

        [Test]
        public void ApplySettings()
        {
            var newSettings = new DisplaySettings
            {
                vSync = true,
                windowMode = WindowMode.Borderless,
                resolutionSizeIndex = 3
            };

            var settingsByKey = GivenNoStoredSettings();

            var repository = new PlayerPrefsDesktopDisplaySettingsRepository(settingsByKey,
                GetDefaultDisplaySettings());

            var onChangedCalled = false;
            repository.OnChanged += x => onChangedCalled = true;

            repository.Apply(newSettings);
            var settings = repository.Data;

            Assert.AreEqual(newSettings, settings);
            Assert.IsTrue(onChangedCalled);
        }

        [Test]
        public void SaveSettings()
        {
            var newSettings = new DisplaySettings
            {
                vSync = true,
                windowMode = WindowMode.Borderless,
                resolutionSizeIndex = 3
            };

            var settingsByKey = GivenNoStoredSettings();

            var repository = new PlayerPrefsDesktopDisplaySettingsRepository(settingsByKey,
                GetDefaultDisplaySettings());

            WhenSettingsAreSaved(repository, newSettings);

            ThenSettingsAreSaved(settingsByKey, newSettings);
        }

        private DisplaySettings WhenGetSettings(PlayerPrefsDesktopDisplaySettingsRepository repository) =>
            repository.Data;

        private void WhenSettingsAreSaved(PlayerPrefsDesktopDisplaySettingsRepository repository,
            DisplaySettings newSettings)
        {
            repository.Apply(newSettings);
            repository.Save();
        }

        private void ThenSettingsAreSaved(IPlayerPrefsSettingsByKey settingsByKey, DisplaySettings settings)
        {
            settingsByKey.Received(1).SetBool(PlayerPrefsDesktopDisplaySettingsRepository.VSYNC, settings.vSync);

            settingsByKey.Received(1)
                         .SetInt(PlayerPrefsDesktopDisplaySettingsRepository.RESOLUTION_SIZE_INDEX,
                              settings.resolutionSizeIndex);

            settingsByKey.Received(1)
                         .SetEnum(PlayerPrefsDesktopDisplaySettingsRepository.WINDOW_MODE,
                              settings.windowMode);

            settingsByKey.Received(1)
                         .SetInt(PlayerPrefsDesktopDisplaySettingsRepository.FPS_CAP,
                              settings.fpsCapIndex);
        }

        private IPlayerPrefsSettingsByKey GivenStoredSettings(DisplaySettings settings)
        {
            var settingsByKey = Substitute.For<IPlayerPrefsSettingsByKey>();

            settingsByKey.GetBool(PlayerPrefsDesktopDisplaySettingsRepository.VSYNC, Arg.Any<bool>())
                         .Returns(settings.vSync);

            settingsByKey.GetInt(PlayerPrefsDesktopDisplaySettingsRepository.RESOLUTION_SIZE_INDEX, Arg.Any<int>())
                         .Returns(settings.resolutionSizeIndex);

            settingsByKey.GetEnum(PlayerPrefsDesktopDisplaySettingsRepository.WINDOW_MODE, Arg.Any<WindowMode>())
                         .Returns(settings.windowMode);

            settingsByKey.GetInt(PlayerPrefsDesktopDisplaySettingsRepository.FPS_CAP, Arg.Any<int>())
                         .Returns(settings.fpsCapIndex);

            return settingsByKey;
        }

        private IPlayerPrefsSettingsByKey GivenNoStoredSettings()
        {
            var settingsByKey = Substitute.For<IPlayerPrefsSettingsByKey>();

            settingsByKey.GetBool(Arg.Any<string>(), Arg.Any<bool>())
                         .Returns(call => call[1]);

            settingsByKey.GetInt(Arg.Any<string>(), Arg.Any<int>())
                         .Returns(call => call[1]);

            settingsByKey.GetEnum(Arg.Any<string>(), Arg.Any<WindowMode>())
                         .Returns(call => call[1]);

            return settingsByKey;
        }

        private DisplaySettings GetDefaultDisplaySettings()
        {
            return new DisplaySettings
            {
                windowMode = WindowMode.FullScreen,
                resolutionSizeIndex = -1,
                vSync = false,
                fpsCapIndex = 0
            };
        }
    }
}
