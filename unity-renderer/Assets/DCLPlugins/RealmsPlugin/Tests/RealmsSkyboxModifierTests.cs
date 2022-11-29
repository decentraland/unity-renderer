using DCL;
using Decentraland.Bff;
using NUnit.Framework;
using System;

namespace DCLPlugins.RealmsPlugin
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

        [TestCase(SkyboxMode.Dynamic)]
        [TestCase(SkyboxMode.HoursFixedByUser)]
        [TestCase(SkyboxMode.HoursFixedByWorld)]
        public void WhenSwitchedFromWorldWithFixedHoursToOneWithoutThenHoursAreReturnedToTheCachedState(SkyboxMode cachedMode)
        {
            // Arrange
            var skyboxConfig = new DataStore_SkyboxConfig();
            skyboxConfig.mode.Set(cachedMode);

            var realmsSkyboxModifier = new RealmsSkyboxModifier(skyboxConfig);

            // Act
            realmsSkyboxModifier.OnEnteredRealm(false, AboutResponseWithFixedHours(60000));
            realmsSkyboxModifier.OnEnteredRealm(false, AboutResponse());

            // Assert
            Assert.That(skyboxConfig.mode.Get(), Is.EqualTo(cachedMode));
        }

        private static AboutResponse AboutResponseWithFixedHours(float fixedHour) =>
            new AboutResponse
            {
                Bff = new AboutResponse.Types.BffInfo(),
                Comms = new AboutResponse.Types.CommsInfo(),
                Configurations = new AboutResponse.Types.AboutConfiguration
                {
                    Skybox = new AboutResponse.Types.SkyboxConfiguration
                    {
                        FixedHour = fixedHour,
                    },
                },
            };

        private static AboutResponse AboutResponse() =>
            new AboutResponse
            {
                Bff = new AboutResponse.Types.BffInfo(),
                Comms = new AboutResponse.Types.CommsInfo(),
                Configurations = new AboutResponse.Types.AboutConfiguration
                {
                    Skybox = new AboutResponse.Types.SkyboxConfiguration(),
                },
            };
    }
}
