using Cysharp.Threading.Tasks;
using DCL.Chat;
using DCL.Chat.Channels;
using DCL.Tasks;
using SocialFeaturesAnalytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Channel = DCL.Chat.Channels.Channel;

namespace DCL.Social.Chat.Channels
{
    public class JoinChannelComponentController : IDisposable
    {
        internal readonly IJoinChannelComponentView view;
        internal readonly IChatController chatController;
        private readonly DataStore dataStore;
        internal readonly DataStore_Channels channelsDataStore;
        private readonly ISocialAnalytics socialAnalytics;
        private readonly BaseVariable<(string playerId, string source)> currentPlayerInfoCardId;
        private readonly IChannelsFeatureFlagService channelsFeatureFlagService;
        private CancellationTokenSource joinChannelCancellationToken = new ();
        private string channelName;

        public JoinChannelComponentController(
            IJoinChannelComponentView view,
            IChatController chatController,
            DataStore dataStore,
            ISocialAnalytics socialAnalytics,
            IChannelsFeatureFlagService channelsFeatureFlagService)
        {
            this.view = view;
            this.chatController = chatController;
            this.dataStore = dataStore;
            channelsDataStore = dataStore.channels;
            this.socialAnalytics = socialAnalytics;
            this.currentPlayerInfoCardId = dataStore.HUDs.currentPlayerId;
            this.channelsFeatureFlagService = channelsFeatureFlagService;

            channelsDataStore.currentJoinChannelModal.OnChange += OnChannelToJoinChanged;
            this.view.OnCancelJoin += OnCancelJoin;
            this.view.OnConfirmJoin += OnConfirmJoin;
        }

        public void Dispose()
        {
            joinChannelCancellationToken.SafeCancelAndDispose();
            channelsDataStore.currentJoinChannelModal.OnChange -= OnChannelToJoinChanged;
            view.OnCancelJoin -= OnCancelJoin;
            view.OnConfirmJoin -= OnConfirmJoin;
        }

        private void OnChannelToJoinChanged(string currentChannelName, string previousChannelName)
        {
            if (!channelsFeatureFlagService.IsChannelsFeatureEnabled())
                return;

            if (string.IsNullOrEmpty(currentChannelName))
                return;

            currentChannelName = currentChannelName.Replace("#", "")
                                                   .Replace("~", "")
                                                   .ToLower();

            Channel alreadyJoinedChannel = chatController.GetAllocatedChannelByName(currentChannelName);

            if (alreadyJoinedChannel is { Joined: true })
            {
                OpenChannelWindow(alreadyJoinedChannel);
                return;
            }

            channelName = currentChannelName;
            view.SetChannel(currentChannelName);
            view.HideLoading();
            view.Show();
        }

        private void OnCancelJoin()
        {
            if (channelsDataStore.channelJoinedSource.Get() == ChannelJoinedSource.Link)
                socialAnalytics.SendChannelLinkClicked(channelName, false, GetChannelLinkSource());

            view.Hide();
            channelsDataStore.currentJoinChannelModal.Set(null);
        }

        private void OnConfirmJoin(string channelName)
        {
            channelName = channelName
                         .Replace("#", "")
                         .Replace("~", "")
                         .ToLower();

            async UniTaskVoid JoinAsync(string channelName, CancellationToken cancellationToken = default)
            {
                try
                {
                    Channel alreadyJoinedChannel = chatController.GetAllocatedChannelByName(channelName);

                    if (alreadyJoinedChannel != null)
                    {
                        if (!alreadyJoinedChannel.Joined)
                        {
                            view.ShowLoading();
                            alreadyJoinedChannel = await chatController.JoinOrCreateChannelAsync(alreadyJoinedChannel.ChannelId, cancellationToken);
                        }

                        OpenChannelWindow(alreadyJoinedChannel);
                    }
                    else
                    {
                        view.ShowLoading();

                        (string pageToken, Channel[] channels) = await chatController.GetChannelsByNameAsync(1, channelName, cancellationToken: cancellationToken);
                        Channel channelToJoin = channels.FirstOrDefault(channel => channel.Name == channelName);

                        if (channelToJoin != null)
                        {
                            channelToJoin = await chatController.JoinOrCreateChannelAsync(channelToJoin.ChannelId, cancellationToken);
                            OpenChannelWindow(channelToJoin);
                        }
                        else
                            ShowErrorToast("The channel you are trying to access does not exist");
                    }
                }
                catch (ChannelException)
                {
                    ShowErrorToast("There was a problem trying to process your request, try again later");
                    throw;
                }
                finally
                {
                    if (channelsDataStore.channelJoinedSource.Get() == ChannelJoinedSource.Link)
                        socialAnalytics.SendChannelLinkClicked(channelName, true, GetChannelLinkSource());

                    view.HideLoading();
                    view.Hide();
                    channelsDataStore.currentJoinChannelModal.Set(null);
                }
            }

            joinChannelCancellationToken = joinChannelCancellationToken.SafeRestart();
            JoinAsync(channelName, joinChannelCancellationToken.Token).Forget();
        }

        private void ShowErrorToast(string message)
        {
            dataStore.notifications.DefaultErrorNotification.Set(message, true);
        }

        private void OpenChannelWindow(Channel channel)
        {
            dataStore.channels.channelToBeOpened.Set(channel.ChannelId, true);
        }

        private ChannelLinkSource GetChannelLinkSource()
        {
            if (dataStore.exploreV2.isOpen.Get()
                && dataStore.exploreV2.placesAndEventsVisible.Get()
                && dataStore.exploreV2.isSomeModalOpen.Get())
            {
                switch (dataStore.exploreV2.currentVisibleModal.Get())
                {
                    case ExploreV2CurrentModal.Events:
                        return ChannelLinkSource.Event;
                    case ExploreV2CurrentModal.Places:
                        return ChannelLinkSource.Place;
                }
            }

            if (!string.IsNullOrEmpty(currentPlayerInfoCardId.Get().playerId))
                return ChannelLinkSource.Profile;

            var visibleTaskbarPanels = dataStore.HUDs.visibleTaskbarPanels.Get();

            if (visibleTaskbarPanels.Any(panel => panel == "PrivateChatChannel"
                                                  || panel == "ChatChannel"
                                                  || panel == "PublicChatChannel"))
                return ChannelLinkSource.Chat;

            return ChannelLinkSource.Unknown;
        }
    }
}
