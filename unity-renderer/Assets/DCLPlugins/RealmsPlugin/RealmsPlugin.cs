using System.Collections.Generic;
using System.Linq;
using DCL;
using Decentraland.Bff;
using UnityEngine;
using Variables.RealmsInfo;

namespace DCLPlugins.RealmsPlugin
{
    public class RealmsPlugin : IPlugin
    {
        private BaseCollection<RealmModel> realmsList => DataStore.i.realm.realmsInfo;
        private BaseVariable<AboutResponse> realmAboutConfiguration => DataStore.i.realm.playerRealmAbout;
        private AboutResponse currentConfiguration;
        
        internal List<IRealmsModifier> realmsModifiers;

        public RealmsPlugin()
        {
            Debug.Log("CREATED REALMS PLUGINS");
            
            realmsModifiers = new List<IRealmsModifier>() { new RealmsBlockerModifier() };
            
            realmAboutConfiguration.OnChange += RealmChanged;
            realmsList.OnSet += RealmListSet;
        }

        private void RealmListSet(IEnumerable<RealmModel> _)
        {
            Debug.Log("CALLING REALM LIST SET");
            
            if (currentConfiguration != null)
                SetRealmModifiers();
        }

        private void RealmChanged(AboutResponse current, AboutResponse _)
        {
            Debug.Log("CALLING REALM CHANGE");

            currentConfiguration = current;
            if (realmsList.Count().Equals(0))
                return;

            SetRealmModifiers();
        }

        private void SetRealmModifiers()
        {
            List<RealmModel> currentRealmsList = realmsList.Get().ToList();
            bool isCatalyst = currentRealmsList.FirstOrDefault(r => r.serverName == currentConfiguration.Configurations.RealmName) != null;
            realmsModifiers.ForEach(e => e.OnEnteredRealm(isCatalyst, currentConfiguration));
        }

        public void Dispose()
        {
            realmsModifiers.ForEach(e => e.Dispose());
            
            realmAboutConfiguration.OnChange -= RealmChanged;
            realmsList.OnSet -= RealmListSet;
        }
    }
}