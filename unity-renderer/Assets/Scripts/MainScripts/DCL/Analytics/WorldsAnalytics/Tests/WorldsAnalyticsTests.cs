using DCL;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

namespace WorldsFeaturesAnalytics
{
    public class WorldsAnalyticsTests
    {
        private WorldsAnalytics worldsAnalytics;
        private IAnalytics analytics;

        [SetUp]
        public void SetUp()
        {
            analytics = Substitute.For<IAnalytics>();
            worldsAnalytics = new WorldsAnalytics(DataStore.i.common, DataStore.i.realm, analytics);
            RealmPluginTestsUtils.SetRealm(true);
        }

        [TearDown]
        public void TearDown() =>
            worldsAnalytics.Dispose();

        [TestCase(false,WorldsAnalytics.EXIT_WORLD)]
        [TestCase(true,WorldsAnalytics.ENTERED_WORLD)]
        public void WorldModified(bool worldEnty, string eventThatShouldBeFired)
        {
            // Act
            RealmPluginTestsUtils.SetRealm(worldEnty);

            // Assert
            analytics.Received(1).SendAnalytic(eventThatShouldBeFired, Arg.Any<Dictionary<string,string>>());
        }

    }
}
