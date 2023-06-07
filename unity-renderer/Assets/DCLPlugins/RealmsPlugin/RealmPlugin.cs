using DCL;
using DCLPlugins.RealmsPlugin;
using System.Collections.Generic;
using Variables.RealmsInfo;
using static Decentraland.Bff.AboutResponse.Types;

namespace DCLPlugins.RealmPlugin
{
    /// <summary>
    /// Contains and triggers the realm modifiers when a new realm has been entered. This is triggered by setting a new AboutConfiguration
    /// </summary>
    public class RealmPlugin : IPlugin
    {
        private BaseVariable<AboutConfiguration> realmAboutConfiguration;
        private List<RealmModel> currentCatalystRealmList;

        internal List<IRealmModifier> realmsModifiers;

        public RealmPlugin(DataStore dataStore)
        {
            this.realmsModifiers = new List<IRealmModifier>
            {
                new RealmBlockerModifier(dataStore.worldBlockers),
                new RealmMinimapModifier(dataStore.HUDs),
                new RealmsSkyboxModifier(dataStore.skyboxConfig),
                new RealmsInfiniteFloorModifier(dataStore.HUDs)
            };

            realmAboutConfiguration = dataStore.realm.playerRealmAboutConfiguration;
            realmAboutConfiguration.OnChange += RealmChanged;
        }

        public void Dispose()
        {
            realmsModifiers.ForEach(rm => rm.Dispose());
            realmAboutConfiguration.OnChange -= RealmChanged;
        }

        private void RealmChanged(AboutConfiguration current, AboutConfiguration _)
        {
            DataStore.i.common.isWorld.Set(current.IsWorld());

            realmsModifiers.ForEach(rm => rm.OnEnteredRealm(current.IsWorld(), current));
        }
    }
}
