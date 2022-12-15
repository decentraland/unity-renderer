using DCL;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorldsFeaturesAnalytics
{
    public class WorldsAnalytics : IWorldsAnalytics
    {
        private const string ENTERED_WORLD = "user_entered_world";
        private const string EXIT_WORLD = "user_exit_world";

        private readonly IAnalytics analytics;
        private bool currentlyInWorld;
        private string currentWorldName;
        private double lastRealmEnteredTime;
        private bool firstRealmEntered;
        private readonly DataStore_Common commonDataStore;


        public WorldsAnalytics(DataStore_Common commonDataStore, IAnalytics analytics)
        {
            this.commonDataStore = commonDataStore;
            this.analytics = analytics;
            commonDataStore.isApplicationQuitting.OnChange += IsApplicationQuittingOnChange;
        }

        private void IsApplicationQuittingOnChange(bool current, bool previous)
        {
            if(currentlyInWorld)
                SendPlayerLeavesWorld(currentWorldName, Time.realtimeSinceStartup - lastRealmEnteredTime, ExitSourceType.ApplicationClosed);

            commonDataStore.isApplicationQuitting.OnChange -= IsApplicationQuittingOnChange;
        }

        private void SendPlayerEnteredWorld(string worldName, WorldAccessType accessType)
        {
            var data = new Dictionary<string, string>
            {
                { "worldName", worldName },
                { "worldAccessType", accessType.ToString() },
            };

            analytics.SendAnalytic(ENTERED_WORLD, data);
        }

        private void SendPlayerLeavesWorld(string worldName, double sessionTimeInSeconds, ExitSourceType exitSourceType)
        {
            var data = new Dictionary<string, string>
            {
                { "worldName", worldName },
                { "sessionTime", sessionTimeInSeconds.ToString() },
                { "exitSourceType", exitSourceType.ToString() },
            };

            analytics.SendAnalytic(EXIT_WORLD, data);
        }

        public void OnEnteredRealm(bool isWorld, string newRealmName)
        {
            if (currentlyInWorld)
                SendPlayerLeavesWorld(currentWorldName, Time.realtimeSinceStartup - lastRealmEnteredTime, commonDataStore.exitedWorldThroughGoBackButton.Get() ? ExitSourceType.GoBackButton : ExitSourceType.Chat);

            if (isWorld)
            {
                currentWorldName = newRealmName;
                SendPlayerEnteredWorld(currentWorldName, firstRealmEntered ? WorldAccessType.URL : WorldAccessType.Chat);
            }

            currentlyInWorld = isWorld;
            lastRealmEnteredTime = Time.realtimeSinceStartup;
            firstRealmEntered = true;
            commonDataStore.exitedWorldThroughGoBackButton.Set(false);
        }
    }
}
