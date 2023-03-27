using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.Chat.Channels;
using DCL.Tasks;
using System.Collections.Generic;

namespace DCL.Chat.HUD
{
    public class ChannelMembersHUDController : IDisposable
    {
        private const int LOAD_TIMEOUT = 2;
        private const int LOAD_PAGE_SIZE = 30;
        private const int MINUTES_FOR_AUTOMATIC_RELOADING = 1;
        private readonly IChatController chatController;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly DataStore_Channels dataStoreChannels;
        private IChannelMembersComponentView view;
        internal DateTime loadStartedTimestamp = DateTime.MinValue;
        private CancellationTokenSource loadingCancellationToken = new CancellationTokenSource();
        private CancellationTokenSource reloadingCancellationToken = new CancellationTokenSource();
        private CancellationTokenSource showMembersCancellationToken = new ();
        private string currentChannelId;
        private int lastLimitRequested;
        private bool isSearching;
        private bool isVisible;
        private int currentMembersCount;

        public IChannelMembersComponentView View => view;
        public string CurrentChannelId => currentChannelId;
        public bool IsVisible => isVisible;

        public ChannelMembersHUDController(IChannelMembersComponentView view, IChatController chatController,
            IUserProfileBridge userProfileBridge,
            DataStore_Channels dataStoreChannels)
        {
            this.view = view;
            this.chatController = chatController;
            this.userProfileBridge = userProfileBridge;
            this.dataStoreChannels = dataStoreChannels;
        }

        public void SetChannelId(string channelId)
        {
            if (string.IsNullOrEmpty(channelId))
                return;

            currentChannelId = channelId;
            lastLimitRequested = LOAD_PAGE_SIZE;

            if (isVisible)
            {
                LoadMembers();
                SetAutomaticReloadingActive(true);
                view.ClearSearchInput();
            }
        }

        public void SetMembersCount(int membersCount) =>
            currentMembersCount = membersCount;

        public void Dispose()
        {
            ClearListeners();
            view.Dispose();
            loadingCancellationToken.SafeCancelAndDispose();
            reloadingCancellationToken.SafeCancelAndDispose();
            showMembersCancellationToken.SafeCancelAndDispose();
        }

        public void SetVisibility(bool visible)
        {
            isVisible = visible;

            if (visible)
            {
                LoadMembers();
                SetAutomaticReloadingActive(true);
                view.ClearSearchInput(notify: false);
            }
            else
            {
                SetAutomaticReloadingActive(false);
                ClearListeners();
                view.Hide();
            }
        }

        private void LoadMembers()
        {
            ClearListeners();
            isSearching = false;

            view.OnSearchUpdated += SearchMembers;
            view.OnRequestMoreMembers += LoadMoreMembers;
            chatController.OnUpdateChannelMembers += UpdateChannelMembers;

            view.Show();
            view.ClearAllEntries();
            SetLoadingMoreVisible(false);
            view.ShowLoading();

            loadStartedTimestamp = DateTime.Now;
            string[] channelsToGetInfo = { currentChannelId };
            chatController.GetChannelInfo(channelsToGetInfo);
            chatController.GetChannelMembers(currentChannelId, lastLimitRequested, 0);

            loadingCancellationToken.Cancel();
            loadingCancellationToken = new CancellationTokenSource();
            WaitTimeoutThenHideLoading(loadingCancellationToken.Token).Forget();
        }

        private void SearchMembers(string searchText)
        {
            loadStartedTimestamp = DateTime.Now;
            view.ClearAllEntries();
            SetLoadingMoreVisible(false);
            view.ShowLoading();

            isSearching = !string.IsNullOrEmpty(searchText);

            if (!isSearching)
            {
                chatController.GetChannelMembers(currentChannelId, lastLimitRequested, 0);
                SetAutomaticReloadingActive(true);
                view.HideResultsHeader();
            }
            else
            {
                chatController.GetChannelMembers(currentChannelId, LOAD_PAGE_SIZE, 0, searchText);
                SetAutomaticReloadingActive(false);
                view.ShowResultsHeader();
            }

            loadingCancellationToken.Cancel();
            loadingCancellationToken = new CancellationTokenSource();
            WaitTimeoutThenHideLoading(loadingCancellationToken.Token).Forget();
        }

        private void UpdateChannelMembers(string channelId, ChannelMember[] channelMembers)
        {
            async UniTaskVoid UpdateChannelMembersAsync(IEnumerable<ChannelMember> channelMembers,
                CancellationToken cancellationToken)
            {
                SetLoadingMoreVisible(true);
                view.HideLoading();

                foreach (ChannelMember member in channelMembers)
                {
                    UserProfile memberProfile = userProfileBridge.Get(member.userId);

                    try { memberProfile ??= await userProfileBridge.RequestFullUserProfileAsync(member.userId, cancellationToken); }
                    catch (Exception e) when (e is not OperationCanceledException)
                    {
                        var fallbackMemberEntry = new ChannelMemberEntryModel
                        {
                            isOnline = member.isOnline,
                            thumnailUrl = "",
                            userId = member.userId,
                            userName = member.userId,
                            isOptionsButtonHidden = member.userId == userProfileBridge.GetOwn().userId
                        };

                        view.Set(fallbackMemberEntry);
                    }
                    finally
                    {
                        dataStoreChannels.SetAvailableMemberInChannel(member.userId, currentChannelId);
                    }

                    ChannelMemberEntryModel userToAdd = new ChannelMemberEntryModel
                    {
                        isOnline = member.isOnline,
                        thumnailUrl = memberProfile.face256SnapshotURL,
                        userId = memberProfile.userId,
                        userName = memberProfile.userName,
                        isOptionsButtonHidden = memberProfile.userId == userProfileBridge.GetOwn().userId
                    };

                    view.Set(userToAdd);
                }

                if (isSearching)
                {
                    SetLoadingMoreVisible(false);

                    if (view.EntryCount > 0)
                        view.ShowResultsHeader();
                    else
                        view.HideResultsHeader();
                }
                else
                    SetLoadingMoreVisible(true);
            }

            UpdateChannelMembersAsync(channelMembers, showMembersCancellationToken.Token).Forget();
        }

        private void LoadMoreMembers()
        {
            if (IsLoading() ||
                isSearching ||
                lastLimitRequested >= currentMembersCount) return;

            loadStartedTimestamp = DateTime.Now;
            SetLoadingMoreVisible(false);
            chatController.GetChannelMembers(currentChannelId, LOAD_PAGE_SIZE, view.EntryCount);

            if (!isSearching)
                lastLimitRequested = LOAD_PAGE_SIZE + view.EntryCount;
        }

        private void SetLoadingMoreVisible(bool isVisible)
        {
            if (isVisible)
            {
                if (lastLimitRequested >= currentMembersCount)
                    view.HideLoadingMore();
                else
                    view.ShowLoadingMore();
            }
            else
                view.HideLoadingMore();
        }

        public void SetAutomaticReloadingActive(bool isActive)
        {
            reloadingCancellationToken.Cancel();

            if (isActive)
            {
                reloadingCancellationToken = new CancellationTokenSource();
                ReloadMembersPeriodically(reloadingCancellationToken.Token).Forget();
            }
        }

        private async UniTask ReloadMembersPeriodically(CancellationToken cancellationToken)
        {
            while (true)
            {
                await UniTask.Delay(MINUTES_FOR_AUTOMATIC_RELOADING * 60 * 1000, cancellationToken: cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    return;

                LoadMembers();
            }
        }

        private bool IsLoading() =>
            (DateTime.Now - loadStartedTimestamp).TotalSeconds < LOAD_TIMEOUT;

        private void ClearListeners()
        {
            view.OnSearchUpdated -= SearchMembers;
            view.OnRequestMoreMembers -= LoadMoreMembers;
            chatController.OnUpdateChannelMembers -= UpdateChannelMembers;
        }

        private async UniTask WaitTimeoutThenHideLoading(CancellationToken cancellationToken)
        {
            await UniTask.Delay(LOAD_TIMEOUT * 1000, cancellationToken: cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            view.HideLoading();
        }
    }
}
