using DCL;
using Sentry;
using System;
using UnityEngine;

namespace DCLPlugins.SentryPlugin
{
    public class SentryController : IDisposable
    {
        private readonly DataStore_Player playerStore;
        private readonly DataStore_Realm realmStore;
        public SentryController(DataStore_Player playerStore, DataStore_Realm realmStore)
        {
            this.playerStore = playerStore;
            this.realmStore = realmStore;

            this.playerStore.playerGridPosition.OnChange += PlayerGridPositionOnOnChange;
            this.playerStore.otherPlayers.OnAdded += OtherPlayersOnChanged;
            this.playerStore.otherPlayers.OnRemoved += OtherPlayersOnChanged;
            this.playerStore.lastTeleportPosition.OnChange += LastTeleportPositionOnOnChange;
            this.realmStore.realmName.OnChange += RealmNameOnOnChange;
        }

        private void RealmNameOnOnChange(string current, string previous)
        {
            SentrySdk.ConfigureScope(scope =>
            {
                scope.SetTag("Current Realm", current);
                scope.SetTag("Previous realm", previous);
            });
        }

        private void LastTeleportPositionOnOnChange(Vector3 current, Vector3 previous)
        {
            SentrySdk.ConfigureScope(scope =>
            {
                scope.SetExtra("Current Teleport Position", $"{current.x},{current.y}");
                scope.SetExtra("Last Teleport Position", $"{previous.x},{previous.y}");
            });
        }

        private void OtherPlayersOnChanged(string _, Player __)
        {
            SentrySdk.ConfigureScope(scope =>
            {
                scope.SetExtra("Total Other Players", $"{DataStore.i.player.otherPlayers.Count()}");
            });
        }

        private void PlayerGridPositionOnOnChange(Vector2Int current, Vector2Int previous)
        {
            SentrySdk.ConfigureScope(scope =>
            {
                scope.SetTag("Current Position", $"{current.x},{current.y}");
                scope.SetTag("Previous Position", $"{previous.x},{previous.y}");
            });
        }

        public void Dispose()
        {
            this.playerStore.playerGridPosition.OnChange -= PlayerGridPositionOnOnChange;
            this.playerStore.otherPlayers.OnAdded -= OtherPlayersOnChanged;
            this.playerStore.otherPlayers.OnRemoved -= OtherPlayersOnChanged;
            this.playerStore.lastTeleportPosition.OnChange -= LastTeleportPositionOnOnChange;
            this.realmStore.realmName.OnChange -= RealmNameOnOnChange;
        }
    }
}
