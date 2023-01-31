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
        private readonly IHub sentryHub;

        public SentryController(DataStore_Player playerStore, DataStore_Realm realmStore, IHub sentryHub)
        {
            this.playerStore = playerStore;
            this.realmStore = realmStore;
            this.sentryHub = sentryHub;

            this.playerStore.playerGridPosition.OnChange += PlayerGridPositionOnOnChange;
            this.playerStore.otherPlayers.OnAdded += OtherPlayersOnChanged;
            this.playerStore.otherPlayers.OnRemoved += OtherPlayersOnChanged;
            this.playerStore.lastTeleportPosition.OnChange += LastTeleportPositionOnOnChange;
            this.realmStore.realmName.OnChange += RealmNameOnOnChange;
        }

        private void RealmNameOnOnChange(string current, string previous)
        {
            if (current == previous) return;
            sentryHub.ConfigureScope(scope =>
            {
                scope.SetTag("Current Realm", current);
                scope.SetTag("Previous Realm", previous);
            });
        }

        private void LastTeleportPositionOnOnChange(Vector3 current, Vector3 previous)
        {
            sentryHub.ConfigureScope(scope =>
            {
                scope.Contexts["Current Teleport Position"] = $"{current.x},{current.y}";
                scope.Contexts["Last Teleport Position"] = $"{previous.x},{previous.y}";
            });
        }

        private void OtherPlayersOnChanged(string _, Player __)
        {
            sentryHub.ConfigureScope(scope => { scope.Contexts["Total Other Players"] = $"{playerStore.otherPlayers.Count()}"; });
        }

        private void PlayerGridPositionOnOnChange(Vector2Int current, Vector2Int previous)
        {
            if (current == previous) return;
            sentryHub.ConfigureScope(scope =>
            {
                scope.SetTag("Current Position", $"{current.x},{current.y}");
                scope.SetTag("Previous Position", $"{previous.x},{previous.y}");
            });
        }

        public void Dispose()
        {
            playerStore.playerGridPosition.OnChange -= PlayerGridPositionOnOnChange;
            playerStore.otherPlayers.OnAdded -= OtherPlayersOnChanged;
            playerStore.otherPlayers.OnRemoved -= OtherPlayersOnChanged;
            playerStore.lastTeleportPosition.OnChange -= LastTeleportPositionOnOnChange;
            realmStore.realmName.OnChange -= RealmNameOnOnChange;
        }
    }
}
