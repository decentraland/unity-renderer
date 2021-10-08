using System.Collections.Generic;
using DCL;
using UnityEngine;

namespace AvatarNamesHUD
{
    public class AvatarNamesHUDController : IHUD
    {
        private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;
        private BaseVariable<bool> avatarNamesVisible => DataStore.i.HUDs.avatarNamesVisible;
        internal InputAction_Trigger toggleHUDAction;

        internal IAvatarNamesHUDView view;
        internal readonly HashSet<string> trackingPlayers = new HashSet<string>();

        internal virtual IAvatarNamesHUDView CreateView() { return AvatarNamesHUDView.CreateView(); }

        public void Initialize()
        {
            view = CreateView();
            view?.Initialize();

            toggleHUDAction = Resources.Load<InputAction_Trigger>("ToggleAvatarNames");
            toggleHUDAction.OnTriggered += OnToggleHudAction;
            otherPlayers.OnAdded += OnOtherPlayersStatusAdded;
            otherPlayers.OnRemoved += OnOtherPlayersStatusRemoved;
            avatarNamesVisible.OnChange += OnAvatarNamesVisibleChange;
            using var enumerator = otherPlayers.Get().GetEnumerator();
            while (enumerator.MoveNext())
            {
                OnOtherPlayersStatusAdded(enumerator.Current.Key, enumerator.Current.Value);
            }
            SetVisibility(false);
        }
        private void OnToggleHudAction(DCLAction_Trigger action) { SetVisibility(!avatarNamesVisible.Get()); }

        internal void OnOtherPlayersStatusAdded(string userId, Player player)
        {
            if (trackingPlayers.Contains(userId))
                return;

            trackingPlayers.Add(userId);
            view?.TrackPlayer(player);
        }

        internal void OnOtherPlayersStatusRemoved(string userId, Player player)
        {
            if (!trackingPlayers.Remove(userId))
                return;

            view?.UntrackPlayer(userId);
        }

        internal void OnAvatarNamesVisibleChange(bool current, bool previous) { SetViewVisibility(current); }

        public void SetVisibility(bool visible) { avatarNamesVisible.Set(visible); }

        private void SetViewVisibility(bool visible) { view?.SetVisibility(visible); }

        public void Dispose()
        {
            toggleHUDAction.OnTriggered -= OnToggleHudAction;
            otherPlayers.OnAdded -= OnOtherPlayersStatusAdded;
            otherPlayers.OnRemoved -= OnOtherPlayersStatusRemoved;
            avatarNamesVisible.OnChange -= OnAvatarNamesVisibleChange;
            view?.Dispose();
        }
    }
}