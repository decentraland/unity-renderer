using DCL;
using DCLPlugins.RealmsPlugin;
using NUnit.Framework;
using System;
using static Decentraland.Bff.AboutResponse.Types;

namespace DCLPlugins.RealmPlugin
{
    public class RealmsSkyboxModifierTests
    {
        [Test]
        public void WhenSwitchedToTheWorldWithFixedHoursThenSkyboxIsInFixedByWorldMode()
        {
            // Arrange
            const float FIXED_HOUR = 60000;

            var skyboxConfig = new DataStore_SkyboxConfig();
            skyboxConfig.mode.Set(SkyboxMode.Dynamic);

            var realmsSkyboxModifier = new RealmsSkyboxModifier(skyboxConfig);

            // Act
            realmsSkyboxModifier.OnEnteredRealm(false, AboutResponseWithFixedHours(FIXED_HOUR));

            // Assert
            Assert.That(skyboxConfig.mode.Get(), Is.EqualTo(SkyboxMode.HoursFixedByWorld));
            Assert.That(skyboxConfig.fixedTime.Get(), Is.EqualTo((float)TimeSpan.FromSeconds(FIXED_HOUR).TotalHours));
        }

        [TestCase(SkyboxMode.Dynamic, 100)]
        [TestCase(SkyboxMode.HoursFixedByUser, 300)]
        [TestCase(SkyboxMode.HoursFixedByWorld, 10)]
        public void WhenSwitchedFromWorldWithFixedHoursToWorldWithoutItThenHoursAreReturnedToTheCachedState(SkyboxMode initialMode, float initialHours)
        {
            // Arrange
            var skyboxConfig = new DataStore_SkyboxConfig();
            skyboxConfig.mode.Set(initialMode);
            skyboxConfig.fixedTime.Set(initialHours);

            var realmsSkyboxModifier = new RealmsSkyboxModifier(skyboxConfig);

            // Act
            realmsSkyboxModifier.OnEnteredRealm(false, AboutResponseWithFixedHours(60000));
            realmsSkyboxModifier.OnEnteredRealm(false, AboutResponse());

            // Assert
            Assert.That(skyboxConfig.mode.Get(), Is.EqualTo(initialMode));
            Assert.That(skyboxConfig.fixedTime.Get(), Is.EqualTo(initialHours));
        }

        [Test]
        public void WhenUserChangFixedHoursBetweenWorldsAndSwitchingFromFixedHoursByWorldThenPreviousValuesShouldBeReturned()
        {
            // Arrange
            const float FIXED_HOUR = 61000;

            var skyboxConfig = new DataStore_SkyboxConfig();
            skyboxConfig.mode.Set(SkyboxMode.Dynamic);

            var realmsSkyboxModifier = new RealmsSkyboxModifier(skyboxConfig);

            // Act
            realmsSkyboxModifier.OnEnteredRealm(false, AboutResponseWithFixedHours(60000));
            realmsSkyboxModifier.OnEnteredRealm(false, AboutResponse());
            skyboxConfig.mode.Set(SkyboxMode.HoursFixedByUser);
            skyboxConfig.fixedTime.Set(FIXED_HOUR);
            realmsSkyboxModifier.OnEnteredRealm(false, AboutResponseWithFixedHours(20000));
            realmsSkyboxModifier.OnEnteredRealm(false, AboutResponse());

            // Assert
            Assert.That(skyboxConfig.mode.Get(), Is.EqualTo(SkyboxMode.HoursFixedByUser));
            Assert.That(skyboxConfig.fixedTime.Get(), Is.EqualTo(FIXED_HOUR));
        }

        private static AboutConfiguration AboutResponseWithFixedHours(float fixedHour) =>
            new () { Skybox = new SkyboxConfiguration { FixedHour = fixedHour } };

        private static AboutConfiguration AboutResponse() =>
            new () { Skybox = new SkyboxConfiguration() };
    }
}
