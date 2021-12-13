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

        public void SearchAnchorPoints(string avatarId, Action<IAvatarAnchorPoints> onSuccess)
        {
            CancelCurrentSearch();

            searchAvatarId = avatarId;

            if (avatarId == ownPlayer.Get().id)
            {
                onSuccess?.Invoke(ownPlayer.Get().anchorPoints);
                return;
            }
            else if (string.IsNullOrEmpty(ownPlayer.Get().id))
            {
                ownPlayer.OnChange += OnOwnPlayerChanged;
            }

            if (otherPlayers.TryGetValue(avatarId, out Player player))
            {
                onSuccess?.Invoke(player.anchorPoints);
                otherPlayers.OnRemoved += OnOtherPlayersRemoved;
                return;
            }

            otherPlayers.OnAdded += OnOtherPlayersAdded;
            onIdFoundCallback = onSuccess;
        }

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