using System;

namespace DCL.Chat.HUD
{
    public class NearbyMembersHUDController : IDisposable
    {
        private readonly IChannelMembersComponentView view;
        private readonly DataStore_Player playerDataStore;
        private readonly IUserProfileBridge userProfileBridge;
        private bool isVisible;
        private string currentSearchText;

        public IChannelMembersComponentView View => view;

        public NearbyMembersHUDController(
            IChannelMembersComponentView view,
            DataStore_Player playerDataStore,
            IUserProfileBridge userProfileBridge)
        {
            this.view = view;
            this.playerDataStore = playerDataStore;
            this.userProfileBridge = userProfileBridge;

            playerDataStore.otherPlayers.OnAdded += OnNearbyPlayersAdded;
            playerDataStore.otherPlayers.OnRemoved += OnNearbyPlayersRemoved;
            view.OnSearchUpdated += OnSearchPlayers;
        }

        public void SetVisibility(bool visible)
        {
            isVisible = visible;

            if (visible)
            {
                view.Show();
                view.ClearSearchInput(false);
                LoadCurrentNearbyPlayers();
            }
            else
                view.Hide();
        }

        public void ClearSearch() =>
            view.ClearSearchInput();

        public void Dispose()
        {
            playerDataStore.otherPlayers.OnAdded -= OnNearbyPlayersAdded;
            playerDataStore.otherPlayers.OnRemoved -= OnNearbyPlayersRemoved;
            view.OnSearchUpdated -= OnSearchPlayers;
            view.Dispose();
        }

        private void LoadCurrentNearbyPlayers(string textFilter = "")
        {
            view.ClearAllEntries();

            foreach (var player in playerDataStore.otherPlayers.Get())
            {
                if (!string.IsNullOrEmpty(textFilter) &&
                    !player.Value.name.Contains(textFilter, StringComparison.OrdinalIgnoreCase))
                    continue;

                OnNearbyPlayersAdded(player.Key, player.Value);
            }
        }

        private void OnNearbyPlayersAdded(string userId, Player player)
        {
            if (!isVisible)
                return;

            if (!string.IsNullOrEmpty(currentSearchText) &&
                !player.name.Contains(currentSearchText, StringComparison.OrdinalIgnoreCase))
                return;

            var otherProfile = userProfileBridge.Get(userId);

            if (otherProfile == null)
                return;

            ChannelMemberEntryModel userToAdd = new ChannelMemberEntryModel
            {
                isOnline = true,
                thumnailUrl = otherProfile.face256SnapshotURL,
                userId = otherProfile.userId,
                userName = otherProfile.userName,
                isOptionsButtonHidden = otherProfile.userId == userProfileBridge.GetOwn().userId,
            };

            view.Set(userToAdd);
        }

        private void OnNearbyPlayersRemoved(string userId, Player _)
        {
            if (!isVisible)
                return;

            view.Remove(userId);
        }

        private void OnSearchPlayers(string searchText)
        {
            currentSearchText = searchText;
            LoadCurrentNearbyPlayers(searchText);
        }
    }
}
