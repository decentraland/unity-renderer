using DCL;
using Decentraland.Bff;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using WorldsFeaturesAnalytics;

namespace DCLPlugins.RealmPlugin
{
    public class RealmPluginTests
    {
        private RealmPlugin realmPlugin;
        private IRealmModifier genericModifier;

        [SetUp]
        public void SetUp()
        {
            realmPlugin = new RealmPlugin(DataStore.i);
            genericModifier = Substitute.For<IRealmModifier>();

            var substituteModifiers = new List<IRealmModifier>
                { genericModifier };

            realmPlugin.realmsModifiers = substituteModifiers;
        }

        [TearDown]
        public void TearDown() =>
            realmPlugin.Dispose();

        [TestCaseSource(nameof(isWorldCases))]
        public void ModifierCalledOnRealmChange(bool isWorld)
        {
            // Act
            RealmPluginTestsUtils.SetRealm(isWorld);

            // Assert
            genericModifier.Received(1).OnEnteredRealm(isWorld, Arg.Any<AboutResponse.Types.AboutConfiguration>());
        }

        private static bool[] isWorldCases = { false, true };
    }
}
