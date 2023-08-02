using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.Chat;
using SocialFeaturesAnalytics;
using UnityEngine;
using Channel = DCL.Chat.Channels.Channel;

namespace DCL.Social.Chat
{
    public class SearchChannelsWindowController : IHUD
    {
        private const int LOAD_TIMEOUT = 2;
        internal const int LOAD_PAGE_SIZE = 15;

        private readonly IChatController chatController;
        private readonly IMouseCatcher mouseCatcher;
        private readonly DataStore dataStore;
        private readonly ISocialAnalytics socialAnalytics;
        private readonly IChannelsFeatureFlagService channelsFeatureFlagService;
        private ISearchChannelsWindowView view;
        private DateTime loadStartedTimestamp = DateTime.MinValue;
        private CancellationTokenSource loadingCancellationToken = new CancellationTokenSource();
        private string paginationToken;
        private string searchText;
        private BaseVariable<HashSet<string>> visibleTaskbarPanels => dataStore.HUDs.visibleTaskbarPanels;
        private bool isSearchingByName;

        public ISearchChannelsWindowView View => view;

        public event Action OnClosed;
        public event Action OnBack;
        public event Action OnOpenChannelCreation;
        public event Action<string> OnOpenChannelLeave;

        public SearchChannelsWindowController(IChatController chatController,
            IMouseCatcher mouseCatcher,
            DataStore dataStore,
            ISocialAnalytics socialAnalytics,
            IChannelsFeatureFlagService channelsFeatureFlagService)
        {
            this.chatController = chatController;
            this.mouseCatcher = mouseCatcher;
            this.dataStore = dataStore;
            this.socialAnalytics = socialAnalytics;
            this.channelsFeatureFlagService = channelsFeatureFlagService;
        }

        public void Initialize(ISearchChannelsWindowView view)
        {
            this.view = view;
            channelsFeatureFlagService.OnAllowedToCreateChannelsChanged += OnAllowedToCreateChannelsChanged;
        }

        public void Dispose()
        {
            ClearListeners();
            view.Dispose();
            loadingCancellationToken.Cancel();
            loadingCancellationToken.Dispose();

            channelsFeatureFlagService.OnAllowedToCreateChannelsChanged -= OnAllowedToCreateChannelsChanged;
        }

        private void SetVisiblePanelList(bool visible)
        {
            HashSet<string> newSet = visibleTaskbarPanels.Get();
            if (visible)
                newSet.Add("SearchChanel");
            else
                newSet.Remove("SearchChanel");

            visibleTaskbarPanels.Set(newSet, true);
        }

        public void SetVisibility(bool visible)
        {
            SetVisiblePanelList(visible);
            if (visible)
            {
                ClearListeners();

                view.ClearSearchInput();

                view.OnSearchUpdated += SearchChannels;
                view.OnRequestMoreChannels += LoadMoreChannels;
                view.OnBack += HandleViewBacked;

                if (mouseCatcher != null)
                    mouseCatcher.OnMouseLock += HandleViewClosed;

                view.OnClose += HandleViewClosed;
                view.OnJoinChannel += HandleJoinChannel;
                view.OnLeaveChannel += HandleLeaveChannel;
                view.OnCreateChannel += OpenChannelCreationWindow;
                view.OnOpenChannel += NavigateToChannel;
                chatController.OnChannelSearchResult += ShowRequestedChannels;
                chatController.OnChannelUpdated += UpdateChannelInfo;

                view.Show();
                view.ClearAllEntries();
                view.ShowLoading();

                loadStartedTimestamp = DateTime.Now;
                paginationToken = null;
                chatController.GetChannels(LOAD_PAGE_SIZE);

                loadingCancellationToken.Cancel();
                loadingCancellationToken = new CancellationTokenSource();
                WaitTimeoutThenHideLoading(loadingCancellationToken.Token).Forget();
            }
            else
            {
                ClearListeners();
                view.Hide();
            }
        }

        private void NavigateToChannel(string channelId)
        {
            var channel = chatController.GetAllocatedChannel(channelId);
            if (channel == null) return;
            if (!channel.Joined) return;
            dataStore.channels.channelToBeOpened.Set(channelId, true);
        }

        private void OpenChannelCreationWindow()
        {
            dataStore.channels.channelJoinedSource.Set(ChannelJoinedSource.Search);
            OnOpenChannelCreation?.Invoke();
        }

        private void SearchChannels(string searchText)
        {
            loadStartedTimestamp = DateTime.Now;
            paginationToken = null;
            view.ClearAllEntries();
            view.HideLoadingMore();
            view.ShowLoading();
            view.HideResultsHeader();
            view.HideCreateChannelOnSearch();
            this.searchText = searchText;
            isSearchingByName = !string.IsNullOrEmpty(searchText);

            if (!isSearchingByName)
                chatController.GetChannels(LOAD_PAGE_SIZE);
            else
            {
                chatController.GetChannelsByName(LOAD_PAGE_SIZE, searchText);
                socialAnalytics.SendChannelSearch(searchText);
            }

            loadingCancellationToken.Cancel();
            loadingCancellationToken = new CancellationTokenSource();
            WaitTimeoutThenHideLoading(loadingCancellationToken.Token).Forget();
        }

        private void ShowRequestedChannels(string paginationToken, Channel[] channels)
        {
            this.paginationToken = paginationToken;

            foreach (var channel in channels)
                view.Set(channel);

            if (isSearchingByName)
            {
                view.HideLoadingMore();

                if (view.EntryCount > 0)
                {
                    view.ShowResultsHeader();
                    view.ShowCreateChannelOnSearch();
                }
                else
                {
                    view.HideResultsHeader();
                    view.HideCreateChannelOnSearch();
                }
            }
            else
                view.ShowLoadingMore();
        }

        private void UpdateChannelInfo(Channel updatedChannel) => view.Set(updatedChannel);

        private void LoadMoreChannels()
        {
            if (IsLoading()) return;
            if (string.IsNullOrEmpty(paginationToken)) return;

            loadStartedTimestamp = DateTime.Now;
            view.HideLoadingMore();

            if (string.IsNullOrEmpty(searchText))
                chatController.GetChannels(LOAD_PAGE_SIZE, paginationToken);
            else
                chatController.GetChannelsByName(LOAD_PAGE_SIZE, searchText, paginationToken);
        }

        private bool IsLoading() => (DateTime.Now - loadStartedTimestamp).TotalSeconds < LOAD_TIMEOUT;

        private void HandleViewClosed() => OnClosed?.Invoke();

        private void HandleViewBacked() => OnBack?.Invoke();

        private void ClearListeners()
        {
            chatController.OnChannelSearchResult -= ShowRequestedChannels;
            chatController.OnChannelUpdated -= UpdateChannelInfo;
            view.OnSearchUpdated -= SearchChannels;
            view.OnRequestMoreChannels -= LoadMoreChannels;
            view.OnBack -= HandleViewBacked;
            view.OnClose -= HandleViewClosed;
            view.OnJoinChannel -= HandleJoinChannel;
            view.OnLeaveChannel -= HandleLeaveChannel;
            view.OnCreateChannel -= OpenChannelCreationWindow;
            view.OnOpenChannel -= NavigateToChannel;
        }

        private async UniTask WaitTimeoutThenHideLoading(CancellationToken cancellationToken)
        {
            await UniTask.Delay(LOAD_TIMEOUT * 1000, cancellationToken: cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            view.HideLoading();
        }

        private void HandleJoinChannel(string channelId)
        {
            dataStore.channels.channelJoinedSource.Set(ChannelJoinedSource.Search);
            chatController.JoinOrCreateChannel(channelId);
        }

        private void HandleLeaveChannel(string channelId)
        {
            dataStore.channels.channelLeaveSource.Set(ChannelLeaveSource.Search);
            OnOpenChannelLeave?.Invoke(channelId);
        }

        private void OnAllowedToCreateChannelsChanged(bool isAllowed) => view.SetCreateChannelButtonsActive(isAllowed);
    }
}
