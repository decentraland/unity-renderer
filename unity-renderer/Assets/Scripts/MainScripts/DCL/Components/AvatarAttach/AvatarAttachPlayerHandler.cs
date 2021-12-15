using System;

namespace DCL.Components
{
    internal class AvatarAttachPlayerHandler : IDisposable
    {
        public event Action onAvatarDisconnect;

        private string searchAvatarId;
        private Action<IAvatarAnchorPoints> onIdFoundCallback;

        private IBaseVariable<Player> ownPlayer => DataStore.i.player.ownPlayer;
        private IBaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;

        /// <summary>
        /// Search for an `avatarId` that could be the own player or any other player.
        /// If that `avatarId` is loaded it will call `onSuccess`, otherwise it will wait
        /// for that `avatarId` to be loaded
        /// </summary>
        public void SearchAnchorPoints(string avatarId, Action<IAvatarAnchorPoints> onSuccess)
        {
            CancelCurrentSearch();

            if (string.IsNullOrEmpty(avatarId))
                return;

            searchAvatarId = avatarId;
            string ownPlayerId = ownPlayer.Get()?.id;

            // NOTE: ownPlayerId null means player avatar is not loaded yet
            // so we subscribe to the event for when player avatar is set.
            if (string.IsNullOrEmpty(ownPlayerId))
            {
                ownPlayer.OnChange += OnOwnPlayerChanged;
            }
            else if (avatarId == ownPlayerId)
            {
                onSuccess?.Invoke(ownPlayer.Get().anchorPoints);
                return;
            }

            // If `avatarId` does not match ownPlayer id then we check against other players.
            if (otherPlayers.TryGetValue(avatarId, out Player player))
            {
                onSuccess?.Invoke(player.anchorPoints);
                otherPlayers.OnRemoved += OnOtherPlayersRemoved;
                return;
            }

            // If `avatarId` was not found yet, then we subscribe and wait for a player of that id to connect.
            otherPlayers.OnAdded += OnOtherPlayersAdded;
            onIdFoundCallback = onSuccess;
        }

        /// <summary>
        /// Cancel the active search/waiting of the `avatarId`
        /// </summary>
        public void CancelCurrentSearch()
        {
            otherPlayers.OnAdded -= OnOtherPlayersAdded;
            ownPlayer.OnChange -= OnOwnPlayerChanged;
            onIdFoundCallback = null;
            searchAvatarId = null;
        }

        public void Dispose()
        {
            CancelCurrentSearch();
            otherPlayers.OnRemoved -= OnOtherPlayersRemoved;
        }

        private void OnOwnPlayerChanged(Player player, Player prev)
        {
            if (player.id != searchAvatarId)
            {
                return;
            }
            onIdFoundCallback?.Invoke(player.anchorPoints);
            CancelCurrentSearch();
        }

        private void OnOtherPlayersAdded(string id, Player player)
        {
            if (id != searchAvatarId)
            {
                return;
            }
            onIdFoundCallback?.Invoke(player.anchorPoints);
            otherPlayers.OnRemoved += OnOtherPlayersRemoved;
            CancelCurrentSearch();
        }

        private void OnOtherPlayersRemoved(string id, Player player)
        {
            if (id != searchAvatarId)
            {
                return;
            }
            onAvatarDisconnect?.Invoke();
        }
    }
}