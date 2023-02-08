﻿using DCL;
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
        private static string prefix = "explorer.";
        private Vector3 currentPlayerPosition, previousPlayerPosition;

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
                scope.SetTag($"{prefix}current_realm", current);
                scope.SetTag($"{prefix}previous_realm", previous);
            });
        }

        private void LastTeleportPositionOnOnChange(Vector3 current, Vector3 previous)
        {
            currentPlayerPosition = current;
            previousPlayerPosition = previous;
            UpdatePlayerContext();
        }

        private void OtherPlayersOnChanged(string _, Player __)
        {
            UpdatePlayerContext();
        }

        private void UpdatePlayerContext()
        {
            sentryHub.ConfigureScope(scope =>
            {
                scope.Contexts[$"{prefix}teleport"] = new
                {
                    current_teleport_position = $"{currentPlayerPosition.x},{currentPlayerPosition.y}",
                    previous_teleport_position = $"{previousPlayerPosition.x},{previousPlayerPosition.y}",
                    total_other_players = this.playerStore.otherPlayers.Count(),
                };
            });
        }

        private void PlayerGridPositionOnOnChange(Vector2Int current, Vector2Int previous)
        {
            if (current == previous) return;
            sentryHub.ConfigureScope(scope =>
            {
                scope.SetTag($"{prefix}current_position", $"{current.x},{current.y}");
                scope.SetTag($"{prefix}previous_position", $"{previous.x},{previous.y}");
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
