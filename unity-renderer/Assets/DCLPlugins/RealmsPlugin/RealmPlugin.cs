using DCL;
using Decentraland.Bff;
using System.Collections.Generic;
using System.Linq;
using Variables.RealmsInfo;

namespace DCLPlugins.RealmPlugin
{
    public class RealmPlugin : IPlugin
    {
        private BaseVariable<AboutResponse.Types.AboutConfiguration> realmAboutConfigurationConfiguration => DataStore.i.realm.playerRealmAboutConfiguration;

        private List<RealmModel> currentCatalystRealmList;

        internal List<IRealmModifier> realmsModifiers;

        public RealmPlugin(DataStore dataStore)
        {
            realmsModifiers = new List<IRealmModifier>
                { new RealmBlockerModifier(dataStore.worldBlockers), new RealmMinimapModifier(dataStore.HUDs) };

            realmAboutConfigurationConfiguration.OnChange += RealmChanged;
        }

        private void RealmChanged(AboutResponse.Types.AboutConfiguration current, AboutResponse.Types.AboutConfiguration _)
        {
            bool isWorld = current.ScenesUrn.Any()
                           && string.IsNullOrEmpty(current.CityLoaderContentServer);
            realmsModifiers.ForEach(e => e.OnEnteredRealm(isWorld, current));
        }

        public void Dispose()
        {
            realmsModifiers.ForEach(e => e.Dispose());

            realmAboutConfigurationConfiguration.OnChange -= RealmChanged;
        }
    }
}
