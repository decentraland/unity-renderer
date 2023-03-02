using DCL;
using DCLPlugins.RealmPlugin;
using System.Collections.Generic;
using UnityEngine;
using static Decentraland.Bff.AboutResponse.Types;

namespace WorldsFeaturesAnalytics
{
    public class WorldsAnalytics : IWorldsAnalytics
    {
        internal const string ENTERED_WORLD = "user_entered_world";
        internal const string EXIT_WORLD = "user_exit_world";

        private readonly DataStore_Common commonDataStore;
        private readonly DataStore_Realm realmDataStore;

        private readonly IAnalytics analytics;
        private bool currentlyInWorld;
        private string currentWorldName;
        private double lastRealmEnteredTime;
        private bool firstRealmEntered;

        public WorldsAnalytics(DataStore_Common commonDataStore, DataStore_Realm realmDataStore, IAnalytics analytics)
        {
            this.commonDataStore = commonDataStore;
            this.analytics = analytics;
            this.realmDataStore = realmDataStore;

            realmDataStore.playerRealmAboutConfiguration.OnChange += OnEnteredRealm;
        }

        public void Initialize() { }

        public void Dispose()
        {
            if (currentlyInWorld)
                SendPlayerLeavesWorld(currentWorldName, Time.realtimeSinceStartup - lastRealmEnteredTime, ExitType.ApplicationClosed);

            realmDataStore.playerRealmAboutConfiguration.OnChange -= OnEnteredRealm;
        }

        private void OnEnteredRealm(AboutConfiguration current, AboutConfiguration previous)
        {
            if (currentlyInWorld)
                SendPlayerLeavesWorld(currentWorldName, Time.realtimeSinceStartup - lastRealmEnteredTime, commonDataStore.exitedWorldThroughGoBackButton.Get() ? ExitType.GoBackButton : ExitType.Chat);

            if (current.IsWorld())
            {
                currentWorldName = current.RealmName;
                SendPlayerEnteredWorld(currentWorldName, firstRealmEntered ? AccessType.Chat : AccessType.URL);
            }

            currentlyInWorld = current.IsWorld();
            lastRealmEnteredTime = Time.realtimeSinceStartup;
            firstRealmEntered = true;
            commonDataStore.exitedWorldThroughGoBackButton.Set(false);
        }

        private void SendPlayerEnteredWorld(string worldName, AccessType accessType)
        {
            var data = new Dictionary<string, string>
            {
                { "worldName", worldName },
                { "accessType", accessType.ToString() },
            };

            analytics.SendAnalytic(ENTERED_WORLD, data);
        }

        private void SendPlayerLeavesWorld(string worldName, double sessionTimeInSeconds, ExitType exitType)
        {
            var data = new Dictionary<string, string>
            {
                { "worldName", worldName },
                { "sessionTime", sessionTimeInSeconds.ToString() },
                { "exitType", exitType.ToString() },
            };

            analytics.SendAnalytic(EXIT_WORLD, data);
        }
    }
}
