using DCL;
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
        internal readonly IUpdateEventHandler updateEventHandler;

        public WorldsAnalytics(DataStore_Common commonDataStore, IAnalytics analytics, IUpdateEventHandler updateEventHandler)
        {
            this.commonDataStore = commonDataStore;
            this.analytics = analytics;
            this.updateEventHandler = updateEventHandler;
            updateEventHandler.AddListener(IUpdateEventHandler.EventType.OnDestroy, ApplicationQuitting);
        }

        private void ApplicationQuitting()
        {
            if (currentlyInWorld)
                SendPlayerLeavesWorld(currentWorldName, Time.realtimeSinceStartup - lastRealmEnteredTime, ExitType.ApplicationClosed);

            updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.OnDestroy, ApplicationQuitting);
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

        public void OnEnteredRealm(bool isWorld, string newRealmName)
        {
            if (currentlyInWorld)
                SendPlayerLeavesWorld(currentWorldName, Time.realtimeSinceStartup - lastRealmEnteredTime, commonDataStore.exitedWorldThroughGoBackButton.Get() ? ExitType.GoBackButton : ExitType.Chat);

            if (isWorld)
            {
                currentWorldName = newRealmName;
                SendPlayerEnteredWorld(currentWorldName, firstRealmEntered ? AccessType.Chat : AccessType.URL);
            }

            currentlyInWorld = isWorld;
            lastRealmEnteredTime = Time.realtimeSinceStartup;
            firstRealmEntered = true;
            commonDataStore.exitedWorldThroughGoBackButton.Set(false);
        }
    }
}
