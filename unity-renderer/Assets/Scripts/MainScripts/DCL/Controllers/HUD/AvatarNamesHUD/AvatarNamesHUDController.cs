using System.Collections.Generic;
using DCL;

namespace AvatarNamesHUD
{
    public class AvatarNamesHUDController : IHUD
    {
        private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;

        internal IAvatarNamesHUDView view;
        internal readonly HashSet<string> trackingPlayers = new HashSet<string>();

        internal virtual IAvatarNamesHUDView CreateView() { return AvatarNamesHUDView.CreateView(); }

        public void Initialize()
        {
            view = CreateView();
            view?.Initialize();

            otherPlayers.OnAdded += OnOtherPlayersStatusAdded;
            otherPlayers.OnRemoved += OnOtherPlayersStatusRemoved;
            using var enumerator = otherPlayers.Get().GetEnumerator();
            while (enumerator.MoveNext())
            {
                OnOtherPlayersStatusAdded(enumerator.Current.Key, enumerator.Current.Value);
            }
        }

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

        public void SetVisibility(bool visible) { view?.SetVisibility(visible); }

        public void Dispose()
        {
            otherPlayers.OnAdded -= OnOtherPlayersStatusAdded;
            otherPlayers.OnRemoved -= OnOtherPlayersStatusRemoved;
            view?.Dispose();
        }
    }
}