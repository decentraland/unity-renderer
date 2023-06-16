using DCL;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

namespace DCLPlugins.RealmPlugin
{
    public class RealmBlockerModifierTests
    {
        private const string ENABLE_GREEN_BLOCKERS_WORLDS_FF = "realms_blockers_in_worlds";

        private RealmBlockerModifier realmBlockerModifer;

        [SetUp]
        public void SetUp()
        {
            realmBlockerModifer = new RealmBlockerModifier(DataStore.i.worldBlockers);
        }

        [TearDown]
        public void TearDown() =>
            realmBlockerModifer.Dispose();

        [TestCaseSource(nameof(GreenBlockerCases))]
        public void GreenBlockerAddedOnRealmChange(bool[] isWorld, int[] requiredLimit)
        {
            for (var i = 0; i < isWorld.Length; i++)
            {
                // Act
                realmBlockerModifer.OnEnteredRealm(isWorld[i], RealmPluginTestsUtils.GetAboutConfiguration(isWorld[i]));

                // Assert
                if (DataStore.i.featureFlags.flags.Get().IsFeatureEnabled(ENABLE_GREEN_BLOCKERS_WORLDS_FF))
                    Assert.AreEqual(DataStore.i.worldBlockers.worldBlockerLimit.Get(), requiredLimit[i]);
            }
        }

        private static object[] GreenBlockerCases =
        {
            new object[] { new[] { false, true, false }, new[] { 0, 2, 0 } },
            new object[] { new[] { true, false, true }, new[] { 2, 0, 2 } },
        };
    }
}
