using DCL.Chat;
using DCL.Chat.Channels;

namespace DCL.Social.Chat.Channels
{
    public class ChannelLinkDetectorController
    {
        private readonly IChannelLinkDetectorView view;
        private readonly IChannelsFeatureFlagService channelsFeatureFlagService;
        private readonly DataStore dataStore;
        private readonly IUserProfileBridge userProfileBridge;

        public ChannelLinkDetectorController(IChannelLinkDetectorView view,
            DataStore dataStore,
            IUserProfileBridge userProfileBridge)
        {
            this.view = view;
            this.dataStore = dataStore;
            this.userProfileBridge = userProfileBridge;

            view.Enable();
            view.OnClicked += OnLinkClicked;
        }

        public void Dispose()
        {
            view.OnClicked -= OnLinkClicked;
        }

        private void OnLinkClicked(string link)
        {
            if (!ChannelUtils.IsAChannel(link)) return;

            if (userProfileBridge.GetOwn().isGuest)
                dataStore.HUDs.connectWalletModalVisible.Set(true);
            else
            {
                dataStore.channels.channelJoinedSource.Set(ChannelJoinedSource.Link);
                dataStore.channels.currentJoinChannelModal.Set(link.ToLower(), true);
            }
        }
    }
}
