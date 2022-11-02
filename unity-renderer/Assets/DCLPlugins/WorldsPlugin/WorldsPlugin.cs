using System.Collections.Generic;
using System.Linq;
using DCL;
using Decentraland.Bff;
using Variables.RealmsInfo;

namespace DCLPlugins.WorldsPlugin
{
    public class WorldsPlugin : IPlugin
    {
        private BaseCollection<RealmModel> realmsList => DataStore.i.realm.realmsInfo;
        private BaseVariable<AboutResponse> realmAboutConfiguration => DataStore.i.realm.playerRealmAbout;
        private AboutResponse currentConfiguration;
        
        internal List<IWorldsModifier> worldsModifiers;

        public WorldsPlugin()
        {
            worldsModifiers = new List<IWorldsModifier>() { new WorldsBlockerModifier() };
            
            realmAboutConfiguration.OnChange += RealmChanged;
            realmsList.OnSet += RealmListSet;
        }

        private void RealmListSet(IEnumerable<RealmModel> _)
        {
            if (currentConfiguration != null)
                SetWorldModifiers();
        }

        private void RealmChanged(AboutResponse current, AboutResponse _)
        {
            currentConfiguration = current;
            if (realmsList.Count().Equals(0))
                return;

            SetWorldModifiers();
        }

        private void SetWorldModifiers()
        {
            List<RealmModel> currentRealmsList = realmsList.Get().ToList();
            bool isCatalyst = currentRealmsList.FirstOrDefault(r => r.serverName == currentConfiguration.Configurations.RealmName) != null;
            worldsModifiers.ForEach(e => e.OnEnteredRealm(isCatalyst, currentConfiguration));
        }

        public void Dispose()
        {
            worldsModifiers.ForEach(e => e.Dispose());
            
            realmAboutConfiguration.OnChange -= RealmChanged;
            realmsList.OnSet -= RealmListSet;
        }
    }
}