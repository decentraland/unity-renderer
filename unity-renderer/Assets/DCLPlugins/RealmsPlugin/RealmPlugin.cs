using DCL;
using Decentraland.Bff;
using System.Collections.Generic;
using System.Linq;
using Variables.RealmsInfo;

namespace DCLPlugins.RealmPlugin
{
    public class RealmPlugin : IPlugin
    {
        private BaseCollection<RealmModel> realmsList => DataStore.i.realm.realmsInfo;
        private BaseVariable<AboutResponse> realmAboutConfiguration => DataStore.i.realm.playerRealmAbout;

        private AboutResponse currentConfiguration;
        private List<RealmModel> currentCatalystRealmList;

        internal List<IRealmModifier> realmsModifiers;

        public RealmPlugin(DataStore dataStore)
        {
            realmsModifiers = new List<IRealmModifier>
                { new RealmBlockerModifier(dataStore), new RealmMinimapModifier(dataStore) };

            realmAboutConfiguration.OnChange += RealmChanged;
            realmsList.OnSet += RealmListSet;
        }

        private void RealmListSet(IEnumerable<RealmModel> _)
        {
            if (realmsList.Count() > 0)
            {
                currentCatalystRealmList = realmsList.Get().ToList();
                SetRealmModifiers();
                realmsList.OnSet -= RealmListSet;
            }
        }

        private void RealmChanged(AboutResponse current, AboutResponse _)
        {
            currentConfiguration = current;
            SetRealmModifiers();
        }

        private void SetRealmModifiers()
        {
            if (currentConfiguration == null || currentCatalystRealmList == null)
                return;

            bool isCatalyst = currentCatalystRealmList.FirstOrDefault(r => r.serverName == currentConfiguration.Configurations.RealmName) != null;
            realmsModifiers.ForEach(e => e.OnEnteredRealm(isCatalyst, currentConfiguration));
        }

        public void Dispose()
        {
            realmsModifiers.ForEach(e => e.Dispose());

            realmAboutConfiguration.OnChange -= RealmChanged;
        }
    }
}
