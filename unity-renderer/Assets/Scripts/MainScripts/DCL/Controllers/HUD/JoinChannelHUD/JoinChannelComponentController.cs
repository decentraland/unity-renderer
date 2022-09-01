using System;
using System.Linq;
using DCL;
using SocialFeaturesAnalytics;

public class JoinChannelComponentController : IDisposable
{
    internal readonly IJoinChannelComponentView joinChannelView;
    internal readonly IChatController chatController;
    private readonly DataStore dataStore;
    internal readonly DataStore_Channels channelsDataStore;
    private readonly ISocialAnalytics socialAnalytics;
    private readonly StringVariable currentPlayerInfoCardId;
    private string channelId;

    public JoinChannelComponentController(
        IJoinChannelComponentView joinChannelView,
        IChatController chatController,
        DataStore dataStore,
        ISocialAnalytics socialAnalytics,
        StringVariable currentPlayerInfoCardId)
    {
        this.joinChannelView = joinChannelView;
        this.chatController = chatController;
        this.dataStore = dataStore;
        channelsDataStore = dataStore.channels;
        this.socialAnalytics = socialAnalytics;
        this.currentPlayerInfoCardId = currentPlayerInfoCardId;

        channelsDataStore.currentJoinChannelModal.OnChange += OnChannelToJoinChanged;
        this.joinChannelView.OnCancelJoin += OnCancelJoin;
        this.joinChannelView.OnConfirmJoin += OnConfirmJoin;
    }

    public void Dispose()
    {
        channelsDataStore.currentJoinChannelModal.OnChange -= OnChannelToJoinChanged;
        joinChannelView.OnCancelJoin -= OnCancelJoin;
        joinChannelView.OnConfirmJoin -= OnConfirmJoin;
    }

    private void OnChannelToJoinChanged(string currentChannelId, string previousChannelId)
    {
        if (string.IsNullOrEmpty(currentChannelId))
            return;

        channelId = currentChannelId;
        joinChannelView.SetChannel(currentChannelId);
        joinChannelView.Show();
    }

    private void OnCancelJoin()
    {
        if (channelsDataStore.channelJoinedSource.Get() == ChannelJoinedSource.Link)
            socialAnalytics.SendChannelLinkClicked(channelId, false, GetChannelLinkSource());
        
        joinChannelView.Hide();
    }

    private void OnConfirmJoin(string channelName)
    {
        channelName = channelName.Replace("#", "").Replace("~", "");
        
        chatController.JoinOrCreateChannel(channelName);
        
        if (channelsDataStore.channelJoinedSource.Get() == ChannelJoinedSource.Link)
            socialAnalytics.SendChannelLinkClicked(channelName, true, GetChannelLinkSource());
        
        joinChannelView.Hide();
        channelsDataStore.currentJoinChannelModal.Set(null);
    }

    private ChannelLinkSource GetChannelLinkSource()
    {
        if (dataStore.exploreV2.isOpen.Get())
        {
            if (dataStore.exploreV2.placesAndEventsVisible.Get())
            {
                if (dataStore.exploreV2.isSomeModalOpen.Get())
                {
                    switch (dataStore.exploreV2.currentVisibleModal.Get())
                    {
                        case ExploreV2CurrentModal.Events:
                            return ChannelLinkSource.Event;
                        case ExploreV2CurrentModal.Places:
                            return ChannelLinkSource.Place;
                    }
                }    
            }
        }
        
        if (!string.IsNullOrEmpty(currentPlayerInfoCardId.Get()))
            return ChannelLinkSource.Profile;

        var visibleTaskbarPanels = dataStore.HUDs.visibleTaskbarPanels.Get();

        if (visibleTaskbarPanels.Any(panel => panel == "PrivateChatChannel"
                                              || panel == "ChatChannel"
                                              || panel == "PublicChatChannel"))
            return ChannelLinkSource.Chat;

        return ChannelLinkSource.Unknown;
    }
}
