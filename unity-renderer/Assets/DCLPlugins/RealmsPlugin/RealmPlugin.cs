using DCL;
using DCLPlugins.RealmsPlugin;
using System.Collections.Generic;
using System.Linq;
using Variables.RealmsInfo;
using WorldsFeaturesAnalytics;
using static Decentraland.Bff.AboutResponse.Types;

namespace DCLPlugins.RealmPlugin
{
    /// <summary>
    /// Contains and triggers the realm modifiers when a new realm has been entered. This is triggered by setting a new AboutConfiguration
    /// </summary>
    public class RealmPlugin : IPlugin
    {
        private BaseVariable<AboutConfiguration> realmAboutConfiguration => DataStore.i.realm.playerRealmAboutConfiguration;
        private List<RealmModel> currentCatalystRealmList;
        private IWorldsAnalytics worldAnalytics;

        internal List<IRealmModifier> realmsModifiers;

        public RealmPlugin(DataStore dataStore, IWorldsAnalytics analytics)
        {
            realmsModifiers = new List<IRealmModifier>
            {
                new RealmBlockerModifier(dataStore.worldBlockers),
                new RealmMinimapModifier(dataStore.HUDs),
                new RealmsSkyboxModifier(dataStore.skyboxConfig),
            };
            worldAnalytics = analytics;
            realmAboutConfiguration.OnChange += RealmChanged;
        }

        public void Dispose()
        {
            realmsModifiers.ForEach(rm => rm.Dispose());
            realmAboutConfiguration.OnChange -= RealmChanged;
        }

        private void RealmChanged(AboutConfiguration current, AboutConfiguration _)
        {
            bool isWorld = current.ScenesUrn.Any() && string.IsNullOrEmpty(current.CityLoaderContentServer);

            DataStore.i.common.isWorld.Set(isWorld);

            worldAnalytics.OnEnteredRealm(isWorld, current.RealmName);
            realmsModifiers.ForEach(rm => rm.OnEnteredRealm(isWorld, current));
        }
    }
}
