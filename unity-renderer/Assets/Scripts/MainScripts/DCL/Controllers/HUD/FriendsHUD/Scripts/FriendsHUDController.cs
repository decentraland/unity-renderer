using Cysharp.Threading.Tasks;
using DCL.Tasks;
using SocialFeaturesAnalytics;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace DCL.Social.Friends
{
    public class FriendsHUDController : IHUD
    {
        private const int LOAD_FRIENDS_ON_DEMAND_COUNT = 30;
        private const int MAX_SEARCHED_FRIENDS = 100;
        private const string NEW_FRIEND_REQUESTS_FLAG = "new_friend_requests";
        private const string ENABLE_QUICK_ACTIONS_FOR_FRIEND_REQUESTS_FLAG = "enable_quick_actions_on_friend_requests";
        private const int GET_FRIENDS_TIMEOUT = 10;

        private readonly Dictionary<string, FriendEntryModel> friends = new ();
        private readonly Dictionary<string, FriendEntryModel> onlineFriends = new ();
        private readonly DataStore dataStore;
        private readonly IFriendsController friendsController;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly ISocialAnalytics socialAnalytics;
        private readonly IChatController chatController;
        private readonly IMouseCatcher mouseCatcher;

        private BaseVariable<HashSet<string>> visibleTaskbarPanels => dataStore.HUDs.visibleTaskbarPanels;
        private bool isNewFriendRequestsEnabled => dataStore.featureFlags.flags.Get().IsFeatureEnabled(NEW_FRIEND_REQUESTS_FLAG); // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
        private bool isQuickActionsForFriendRequestsEnabled => !isNewFriendRequestsEnabled || dataStore.featureFlags.flags.Get().IsFeatureEnabled(ENABLE_QUICK_ACTIONS_FOR_FRIEND_REQUESTS_FLAG);
        private CancellationTokenSource friendOperationsCancellationToken = new ();
        private CancellationTokenSource ensureProfilesCancellationToken = new ();
        private UserProfile ownUserProfile;
        private bool searchingFriends;
        private int lastSkipForFriends;
        private int lastSkipForFriendRequests;

        public bool IsVisible { get; private set; }
        public IFriendsHUDComponentView View { get; private set; }

        public event Action<string> OnPressWhisper;
        public event Action OnOpened;
        public event Action OnClosed;
        public event Action OnViewClosed;

        public FriendsHUDController(DataStore dataStore,
            IFriendsController friendsController,
            IUserProfileBridge userProfileBridge,
            ISocialAnalytics socialAnalytics,
            IChatController chatController,
            IMouseCatcher mouseCatcher)
        {
            this.dataStore = dataStore;
            this.friendsController = friendsController;
            this.userProfileBridge = userProfileBridge;
            this.socialAnalytics = socialAnalytics;
            this.chatController = chatController;
            this.mouseCatcher = mouseCatcher;
        }

        public void Initialize(IFriendsHUDComponentView view, bool isVisible = true)
        {
            View = view;

            view.Initialize(chatController, friendsController, socialAnalytics);
            view.RefreshFriendsTab();
            view.OnFriendRequestApproved += HandleRequestAccepted;
            view.OnCancelConfirmation += HandleRequestCancelled;
            view.OnRejectConfirmation += HandleRequestRejected;
            view.OnFriendRequestSent += HandleRequestFriendship;
            view.OnFriendRequestOpened += OpenFriendRequestDetails;
            view.OnWhisper += HandleOpenWhisperChat;
            view.OnClose += HandleViewClosed;
            view.OnRequireMoreFriends += DisplayMoreFriends;
            view.OnRequireMoreFriendRequests += DisplayMoreFriendRequests;
            view.OnSearchFriendsRequested += SearchFriends;
            view.OnFriendListDisplayed += DisplayFriendsIfAnyIsLoaded;
            view.OnRequestListDisplayed += DisplayFriendRequestsIfAnyIsLoaded;
            view.OnDeleteConfirmation += HandleUnfriend;

            if (mouseCatcher != null)
                mouseCatcher.OnMouseLock += HandleViewClosed;

            ownUserProfile = userProfileBridge.GetOwn();
            ownUserProfile.OnUpdate -= HandleProfileUpdated;
            ownUserProfile.OnUpdate += HandleProfileUpdated;

            friendsController.OnUpdateFriendship += HandleFriendshipUpdated;
            friendsController.OnUpdateUserStatus += HandleUserStatusUpdated;
            friendsController.OnFriendNotFound += OnFriendNotFound;
            friendsController.OnFriendRequestReceived += ShowFriendRequest;

            if (friendsController.IsInitialized)
                view.HideLoadingSpinner();
            else
            {
                view.ShowLoadingSpinner();
                friendsController.OnInitialized -= HandleFriendsInitialized;
                friendsController.OnInitialized += HandleFriendsInitialized;
            }

            ShowOrHideMoreFriendsToLoadHint();
            ShowOrHideMoreFriendRequestsToLoadHint();

            SetVisibility(isVisible);
            IsVisible = isVisible;
        }

        private void SetVisiblePanelList(bool visible)
        {
            HashSet<string> newSet = visibleTaskbarPanels.Get();

            if (visible)
                newSet.Add("FriendsPanel");
            else
                newSet.Remove("FriendsPanel");

            visibleTaskbarPanels.Set(newSet, true);
        }

        public void Dispose()
        {
            friendOperationsCancellationToken?.SafeCancelAndDispose();
            friendOperationsCancellationToken = null;
            ensureProfilesCancellationToken?.SafeCancelAndDispose();
            ensureProfilesCancellationToken = null;

            friendsController.OnInitialized -= HandleFriendsInitialized;
            friendsController.OnUpdateFriendship -= HandleFriendshipUpdated;
            friendsController.OnUpdateUserStatus -= HandleUserStatusUpdated;

            if (View != null)
            {
                View.OnFriendRequestApproved -= HandleRequestAccepted;
                View.OnCancelConfirmation -= HandleRequestCancelled;
                View.OnRejectConfirmation -= HandleRequestRejected;
                View.OnFriendRequestSent -= HandleRequestFriendship;
                View.OnFriendRequestOpened -= OpenFriendRequestDetails;
                View.OnWhisper -= HandleOpenWhisperChat;
                View.OnDeleteConfirmation -= HandleUnfriend;
                View.OnClose -= HandleViewClosed;
                View.OnRequireMoreFriends -= DisplayMoreFriends;
                View.OnRequireMoreFriendRequests -= DisplayMoreFriendRequests;
                View.OnSearchFriendsRequested -= SearchFriends;
                View.OnFriendListDisplayed -= DisplayFriendsIfAnyIsLoaded;
                View.OnRequestListDisplayed -= DisplayFriendRequestsIfAnyIsLoaded;
                View.Dispose();
            }

            if (ownUserProfile != null)
                ownUserProfile.OnUpdate -= HandleProfileUpdated;

            if (userProfileBridge != null)
            {
                foreach (string friendId in friends.Keys)
                {
                    var profile = userProfileBridge.Get(friendId);
                    if (profile == null) continue;
                    profile.OnUpdate -= HandleFriendProfileUpdated;
                }
            }
        }

        public void SetVisibility(bool visible)
        {
            if (IsVisible == visible)
                return;

            IsVisible = visible;
            SetVisiblePanelList(visible);

            if (visible)
            {
                lastSkipForFriends = 0;
                lastSkipForFriendRequests = 0;
                View.ClearAll();
                View.Show();
                UpdateNotificationsCounter();

                foreach (var friend in onlineFriends)
                    View.Set(friend.Key, friend.Value);

                if (View.IsFriendListActive)
                    DisplayMoreFriends();
                else if (View.IsRequestListActive)
                    DisplayMoreFriendRequests();

                OnOpened?.Invoke();
            }
            else
            {
                View.Hide();
                View.DisableSearchMode();
                searchingFriends = false;
                OnClosed?.Invoke();
            }
        }

        private void HandleViewClosed()
        {
            OnViewClosed?.Invoke();
            SetVisibility(false);
        }

        private void HandleFriendsInitialized()
        {
            friendsController.OnInitialized -= HandleFriendsInitialized;
            View.HideLoadingSpinner();

            if (View.IsActive())
            {
                if (View.IsFriendListActive && lastSkipForFriends <= 0)
                    DisplayMoreFriendsAsync(RestartFriendsOperationsCancellationToken()).Forget();
                else if (View.IsRequestListActive && lastSkipForFriendRequests <= 0 && !searchingFriends)
                    DisplayMoreFriendRequestsAsync(RestartFriendsOperationsCancellationToken()).Forget();
            }

            UpdateNotificationsCounter();
        }

        private void HandleProfileUpdated(UserProfile profile) =>
            UpdateBlockStatus(profile).Forget();

        private async UniTask UpdateBlockStatus(UserProfile profile)
        {
            const int ITERATIONS_PER_FRAME = 10;

            var iterations = 0;

            foreach (var friendPair in friends)
            {
                string friendId = friendPair.Key;
                var model = friendPair.Value;

                model.blocked = profile.blocked?.Contains(friendId) ?? false;
                await UniTask.SwitchToMainThread();
                View.UpdateBlockStatus(friendId, model.blocked);

                iterations++;

                if (iterations > 0 && iterations % ITERATIONS_PER_FRAME == 0)
                    await UniTask.NextFrame();
            }
        }

        private void HandleRequestFriendship(string userNameOrId)
        {
            async UniTaskVoid HandleRequestFriendshipAsync(string userNameOrId, CancellationToken cancellationToken)
            {
                if (AreAlreadyFriends(userNameOrId))
                    View.ShowRequestSendError(FriendRequestError.AlreadyFriends);
                else
                {
                    if (isNewFriendRequestsEnabled)
                    {
                        FriendRequest request;

                        try
                        {
                            request = await friendsController.RequestFriendshipAsync(userNameOrId, "", cancellationToken);

                            socialAnalytics.SendFriendRequestSent(request.From, request.To, request.MessageBody?.Length ?? 0,
                                PlayerActionSource.FriendsHUD);
                        }
                        catch (Exception e) when (e is not OperationCanceledException)
                        {
                            e.ReportFriendRequestErrorToAnalyticsAsSender(userNameOrId, PlayerActionSource.FriendsHUD.ToString(),
                                userProfileBridge, socialAnalytics);

                            throw;
                        }

                        ShowFriendRequest(request);
                    }
                    else
                    {
                        friendsController.RequestFriendship(userNameOrId);

                        socialAnalytics.SendFriendRequestSent(ownUserProfile?.userId, userNameOrId, 0,
                            PlayerActionSource.FriendsHUD);
                    }

                    View.ShowRequestSendSuccess();
                }
            }

            HandleRequestFriendshipAsync(userNameOrId, RestartFriendsOperationsCancellationToken()).Forget();
        }

        private bool AreAlreadyFriends(string userNameOrId)
        {
            var userId = userNameOrId;
            var profile = userProfileBridge.GetByName(userNameOrId);

            if (profile != default)
                userId = profile.userId;

            return friendsController != null
                   && friendsController.ContainsStatus(userId, FriendshipStatus.FRIEND);
        }

        private void HandleUserStatusUpdated(string userId, UserStatus status) =>
            UpdateUserStatus(userId, status);

        private void UpdateUserStatus(string userId, UserStatus status)
        {
            switch (status.friendshipStatus)
            {
                case FriendshipStatus.FRIEND:
                    var friend = friends.ContainsKey(userId)
                        ? new FriendEntryModel(friends[userId])
                        : new FriendEntryModel();

                    friend.CopyFrom(status);
                    friend.blocked = IsUserBlocked(userId);
                    friends[userId] = friend;
                    View.Set(userId, friend);

                    if (status.presence == PresenceStatus.ONLINE)
                        onlineFriends[userId] = friend;
                    else
                        onlineFriends.Remove(userId);

                    break;
                case FriendshipStatus.NOT_FRIEND:
                    View.Remove(userId);
                    friends.Remove(userId);
                    onlineFriends.Remove(userId);
                    break;
                case FriendshipStatus.REQUESTED_TO: // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
                    if (isNewFriendRequestsEnabled)
                        return;

                    var sentRequest = friends.ContainsKey(userId)
                        ? new FriendRequestEntryModel(friends[userId], string.Empty, false, 0, isQuickActionsForFriendRequestsEnabled)
                        : new FriendRequestEntryModel { bodyMessage = string.Empty, isReceived = false, timestamp = 0, isShortcutButtonsActive = isQuickActionsForFriendRequestsEnabled };

                    sentRequest.CopyFrom(status);
                    sentRequest.blocked = IsUserBlocked(userId);
                    friends[userId] = sentRequest;
                    onlineFriends.Remove(userId);
                    View.Set(userId, sentRequest);
                    break;
                case FriendshipStatus.REQUESTED_FROM: // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
                    if (isNewFriendRequestsEnabled)
                        return;

                    var receivedRequest = friends.ContainsKey(userId)
                        ? new FriendRequestEntryModel(friends[userId], string.Empty, true, 0, isQuickActionsForFriendRequestsEnabled)
                        : new FriendRequestEntryModel { bodyMessage = string.Empty, isReceived = true, timestamp = 0, isShortcutButtonsActive = isQuickActionsForFriendRequestsEnabled };

                    receivedRequest.CopyFrom(status);
                    receivedRequest.blocked = IsUserBlocked(userId);
                    friends[userId] = receivedRequest;
                    onlineFriends.Remove(userId);
                    View.Set(userId, receivedRequest);
                    break;
            }

            UpdateNotificationsCounter();
            ShowOrHideMoreFriendsToLoadHint();
            ShowOrHideMoreFriendRequestsToLoadHint();
        }

        private void HandleFriendshipUpdated(string userId, FriendshipAction friendshipAction)
        {
            async UniTaskVoid HandleFriendshipUpdatedAsync(string userId, FriendshipAction friendshipAction, CancellationToken cancellationToken)
            {
                var userProfile = userProfileBridge.Get(userId);

                switch (friendshipAction)
                {
                    case FriendshipAction.NONE:
                    case FriendshipAction.REJECTED:
                    case FriendshipAction.CANCELLED:
                    case FriendshipAction.DELETED:
                        RemoveFriendship(userId);
                        break;
                    case FriendshipAction.APPROVED:
                        userProfile = await EnsureProfileOrShowFallbackFriend(userId, userProfile, cancellationToken);

                        var approved = friends.ContainsKey(userId)
                            ? new FriendEntryModel(friends[userId])
                            : new FriendEntryModel();

                        approved.CopyFrom(userProfile);
                        approved.blocked = IsUserBlocked(userId);
                        friends[userId] = approved;
                        View.Set(userId, approved);
                        userProfile.OnUpdate -= HandleFriendProfileUpdated;
                        userProfile.OnUpdate += HandleFriendProfileUpdated;
                        break;
                    case FriendshipAction.REQUESTED_FROM: // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
                        if (isNewFriendRequestsEnabled)
                            return;

                        userProfile = await EnsureProfileOrShowFallbackFriend(userId, userProfile, cancellationToken);

                        var requestReceived = friends.ContainsKey(userId)
                            ? new FriendRequestEntryModel(friends[userId], string.Empty, true, 0, isQuickActionsForFriendRequestsEnabled)
                            : new FriendRequestEntryModel { bodyMessage = string.Empty, isReceived = true, timestamp = 0, isShortcutButtonsActive = isQuickActionsForFriendRequestsEnabled };

                        requestReceived.CopyFrom(userProfile);
                        requestReceived.blocked = IsUserBlocked(userId);
                        friends[userId] = requestReceived;
                        View.Set(userId, requestReceived);
                        userProfile.OnUpdate -= HandleFriendProfileUpdated;
                        userProfile.OnUpdate += HandleFriendProfileUpdated;
                        break;
                    case FriendshipAction.REQUESTED_TO: // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
                        if (isNewFriendRequestsEnabled)
                            return;

                        userProfile = await EnsureProfileOrShowFallbackFriend(userId, userProfile, cancellationToken);

                        var requestSent = friends.ContainsKey(userId)
                            ? new FriendRequestEntryModel(friends[userId], string.Empty, false, 0, isQuickActionsForFriendRequestsEnabled)
                            : new FriendRequestEntryModel { bodyMessage = string.Empty, isReceived = false, timestamp = 0, isShortcutButtonsActive = isQuickActionsForFriendRequestsEnabled };

                        requestSent.CopyFrom(userProfile);
                        requestSent.blocked = IsUserBlocked(userId);
                        friends[userId] = requestSent;
                        View.Set(userId, requestSent);
                        userProfile.OnUpdate -= HandleFriendProfileUpdated;
                        userProfile.OnUpdate += HandleFriendProfileUpdated;
                        break;
                }

                UpdateNotificationsCounter();
                ShowOrHideMoreFriendsToLoadHint();
                ShowOrHideMoreFriendRequestsToLoadHint();
            }

            HandleFriendshipUpdatedAsync(userId, friendshipAction, ensureProfilesCancellationToken.Token).Forget();
        }

        private async UniTask<UserProfile> EnsureProfileOrShowFallbackFriend(string userId, UserProfile userProfile, CancellationToken cancellationToken)
        {
            try { userProfile ??= await userProfileBridge.RequestFullUserProfileAsync(userId, cancellationToken); }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                FriendEntryModel fallbackModel = new ()
                {
                    userId = userId,
                    userName = userId,
                    blocked = IsUserBlocked(userId),
                };

                friends[userId] = fallbackModel;
                View.Set(userId, fallbackModel);

                throw;
            }

            return userProfile;
        }

        private void RemoveFriendship(string userId)
        {
            friends.Remove(userId);
            View.Remove(userId);
            onlineFriends.Remove(userId);
        }

        private void HandleFriendProfileUpdated(UserProfile profile)
        {
            var userId = profile.userId;
            if (!friends.ContainsKey(userId)) return;
            friends[userId].CopyFrom(profile);

            var status = friendsController.GetUserStatus(profile.userId);
            if (status == null) return;

            UpdateUserStatus(userId, status);
        }

        private bool IsUserBlocked(string userId)
        {
            if (ownUserProfile != null && ownUserProfile.blocked != null)
                return ownUserProfile.blocked.Contains(userId);

            return false;
        }

        private void OnFriendNotFound(string name)
        {
            View.DisplayFriendUserNotFound();
        }

        private void UpdateNotificationsCounter()
        {
            if (View.IsActive())
                dataStore.friendNotifications.seenFriends.Set(View.FriendCount);

            dataStore.friendNotifications.pendingFriendRequestCount.Set(friendsController.ReceivedRequestCount);
        }

        private void HandleOpenWhisperChat(FriendEntryModel entry) =>
            OnPressWhisper?.Invoke(entry.userId);

        private void HandleUnfriend(string userId)
        {
            dataStore.notifications.GenericConfirmation.Set(GenericConfirmationNotificationData.CreateUnFriendData(
                UserProfileController.userProfilesCatalog.Get(userId)?.userName,
                () => friendsController.RemoveFriend(userId)), true);
        }

        private void HandleRequestRejected(FriendRequestEntryModel entry)
        {
            HandleRequestRejectedAsync(entry.userId, RestartFriendsOperationsCancellationToken()).Forget();
        }

        private async UniTaskVoid HandleRequestRejectedAsync(string userId, CancellationToken cancellationToken)
        {
            if (isNewFriendRequestsEnabled)
            {
                try
                {
                    FriendRequest request = await friendsController.RejectFriendshipAsync(userId, cancellationToken);

                    socialAnalytics.SendFriendRequestRejected(request.From, request.To,
                        PlayerActionSource.FriendsHUD.ToString(), request.HasBodyMessage);

                    RemoveFriendship(userId);
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    e.ReportFriendRequestErrorToAnalyticsByUserId(userId, PlayerActionSource.FriendsHUD.ToString(),
                        friendsController, socialAnalytics);

                    throw;
                }
            }
            else
            {
                friendsController.RejectFriendship(userId);

                socialAnalytics.SendFriendRequestRejected(ownUserProfile?.userId, userId,
                    PlayerActionSource.FriendsHUD.ToString(), false);
            }

            UpdateNotificationsCounter();
        }

        private async UniTaskVoid HandleRequestCancelledAsync(string userId, CancellationToken cancellationToken)
        {
            if (isNewFriendRequestsEnabled)
            {
                try
                {
                    FriendRequest request = await friendsController.CancelRequestByUserIdAsync(userId, cancellationToken);

                    socialAnalytics.SendFriendRequestCancelled(request.From, request.To,
                        PlayerActionSource.FriendsHUD.ToString());

                    RemoveFriendship(userId);
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    e.ReportFriendRequestErrorToAnalyticsByUserId(userId, PlayerActionSource.FriendsHUD.ToString(),
                        friendsController, socialAnalytics);

                    throw;
                }
            }
            else
            {
                friendsController.CancelRequestByUserId(userId);

                socialAnalytics.SendFriendRequestCancelled(ownUserProfile?.userId, userId,
                    PlayerActionSource.FriendsHUD.ToString());
            }
        }

        private void HandleRequestCancelled(FriendRequestEntryModel entry)
        {
            HandleRequestCancelledAsync(entry.userId, RestartFriendsOperationsCancellationToken()).Forget();
        }

        private void HandleRequestAccepted(FriendRequestEntryModel entry)
        {
            HandleRequestAcceptedAsync(entry.userId, RestartFriendsOperationsCancellationToken()).Forget();
        }

        private async UniTaskVoid HandleRequestAcceptedAsync(string userId, CancellationToken cancellationToken)
        {
            if (isNewFriendRequestsEnabled)
            {
                try
                {
                    FriendRequest request = friendsController.GetAllocatedFriendRequestByUser(userId);
                    request = await friendsController.AcceptFriendshipAsync(request.FriendRequestId, cancellationToken);

                    socialAnalytics.SendFriendRequestApproved(request.From, request.To,
                        PlayerActionSource.FriendsHUD.ToString(),
                        request.HasBodyMessage);
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    e.ReportFriendRequestErrorToAnalyticsByUserId(userId, PlayerActionSource.FriendsHUD.ToString(),
                        friendsController, socialAnalytics);

                    throw;
                }
            }
            else
            {
                friendsController.AcceptFriendship(userId);

                socialAnalytics.SendFriendRequestApproved(ownUserProfile?.userId, userId,
                    PlayerActionSource.FriendsHUD.ToString(), false);
            }
        }

        private void DisplayFriendsIfAnyIsLoaded()
        {
            if (lastSkipForFriends > 0) return;
            if (!friendsController.IsInitialized) return;
            DisplayMoreFriendsAsync(RestartFriendsOperationsCancellationToken()).Forget();
        }

        private void DisplayMoreFriends()
        {
            if (!friendsController.IsInitialized) return;
            DisplayMoreFriendsAsync(RestartFriendsOperationsCancellationToken()).Forget();
        }

        private async UniTask DisplayMoreFriendsAsync(CancellationToken cancellationToken)
        {
            string[] friendsToAdd = await friendsController
                                         .GetFriendsAsync(LOAD_FRIENDS_ON_DEMAND_COUNT, lastSkipForFriends, cancellationToken)
                                         .Timeout(TimeSpan.FromSeconds(GET_FRIENDS_TIMEOUT));

            for (var i = 0; i < friendsToAdd.Length; i++)
                HandleFriendshipUpdated(friendsToAdd[i], FriendshipAction.APPROVED);

            // We are not handling properly the case when the friends are not fetched correctly from server.
            // 'lastSkipForFriends' will have an invalid value.
            // this may happen only on the old flow.. the task operation should throw an exception if anything goes wrong in the new flow
            lastSkipForFriends += LOAD_FRIENDS_ON_DEMAND_COUNT;

            ShowOrHideMoreFriendsToLoadHint();
        }

        private void DisplayMoreFriendRequests()
        {
            if (!friendsController.IsInitialized) return;
            if (searchingFriends) return;
            DisplayMoreFriendRequestsAsync(RestartFriendsOperationsCancellationToken()).Forget();
        }

        private async UniTask DisplayMoreFriendRequestsAsync(CancellationToken cancellationToken)
        {
            if (isNewFriendRequestsEnabled)
            {
                var allFriendRequests = await friendsController.GetFriendRequestsAsync(
                    LOAD_FRIENDS_ON_DEMAND_COUNT, lastSkipForFriendRequests,
                    LOAD_FRIENDS_ON_DEMAND_COUNT, lastSkipForFriendRequests,
                    cancellationToken);

                AddFriendRequests(allFriendRequests);
            }
            else
            {
                // TODO (NEW FRIEND REQUESTS): remove when we don't need to keep the retro-compatibility with the old version
                friendsController.GetFriendRequests(
                    LOAD_FRIENDS_ON_DEMAND_COUNT, lastSkipForFriendRequests,
                    LOAD_FRIENDS_ON_DEMAND_COUNT, lastSkipForFriendRequests);
            }

            // We are not handling properly the case when the friend requests are not fetched correctly from server.
            // 'lastSkipForFriendRequests' will have an invalid value.
            // this may happen only on the old flow.. the task operation should throw an exception if anything goes wrong in the new flow
            lastSkipForFriendRequests += LOAD_FRIENDS_ON_DEMAND_COUNT;

            ShowOrHideMoreFriendRequestsToLoadHint();
        }

        private void AddFriendRequests(List<FriendRequest> friendRequests)
        {
            if (friendRequests == null)
                return;

            foreach (var friendRequest in friendRequests)
                ShowFriendRequest(friendRequest);
        }

        private void ShowFriendRequest(FriendRequest friendRequest)
        {
            async UniTaskVoid ShowFriendRequestAsync(FriendRequest friendRequest, CancellationToken cancellationToken)
            {
                bool isReceivedRequest = friendRequest.IsSentTo(ownUserProfile.userId);
                string userId = isReceivedRequest ? friendRequest.From : friendRequest.To;
                UserProfile userProfile = userProfileBridge.Get(userId);

                try { userProfile ??= await userProfileBridge.RequestFullUserProfileAsync(userId, cancellationToken); }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    FriendRequestEntryModel fallbackModel = new ()
                    {
                        bodyMessage = friendRequest.MessageBody,
                        isReceived = isReceivedRequest,
                        timestamp = friendRequest.Timestamp,
                        isShortcutButtonsActive = isQuickActionsForFriendRequestsEnabled,
                        blocked = IsUserBlocked(userId),
                        userId = userId,
                        userName = userId,
                    };

                    friends[userId] = fallbackModel;
                    onlineFriends.Remove(userId);
                    View.Set(userId, fallbackModel);

                    throw;
                }

                var request = friends.ContainsKey(userId)
                    ? new FriendRequestEntryModel(friends[userId], friendRequest.MessageBody, isReceivedRequest, friendRequest.Timestamp, isQuickActionsForFriendRequestsEnabled)
                    : new FriendRequestEntryModel
                    {
                        bodyMessage = friendRequest.MessageBody,
                        isReceived = isReceivedRequest,
                        timestamp = friendRequest.Timestamp,
                        isShortcutButtonsActive = isQuickActionsForFriendRequestsEnabled
                    };

                request.CopyFrom(userProfile);
                request.blocked = IsUserBlocked(userId);
                friends[userId] = request;
                onlineFriends.Remove(userId);
                View.Set(userId, request);
                userProfile.OnUpdate -= HandleFriendProfileUpdated;
                userProfile.OnUpdate += HandleFriendProfileUpdated;
            }

            ShowFriendRequestAsync(friendRequest, ensureProfilesCancellationToken.Token).Forget();
        }

        private void DisplayFriendRequestsIfAnyIsLoaded()
        {
            if (lastSkipForFriendRequests > 0) return;
            if (!friendsController.IsInitialized) return;
            if (searchingFriends) return;
            DisplayMoreFriendRequestsAsync(RestartFriendsOperationsCancellationToken()).Forget();
        }

        private void ShowOrHideMoreFriendRequestsToLoadHint()
        {
            if (lastSkipForFriendRequests >= friendsController.TotalFriendRequestCount)
                View.HideMoreRequestsToLoadHint();
            else
                View.ShowMoreRequestsToLoadHint(Mathf.Clamp(friendsController.TotalFriendRequestCount - lastSkipForFriendRequests,
                    0,
                    friendsController.TotalFriendRequestCount));
        }

        private void ShowOrHideMoreFriendsToLoadHint()
        {
            if (lastSkipForFriends >= friendsController.TotalFriendCount || searchingFriends)
                View.HideMoreFriendsToLoadHint();
            else
                View.ShowMoreFriendsToLoadHint(Mathf.Clamp(friendsController.TotalFriendCount - lastSkipForFriends,
                    0,
                    friendsController.TotalFriendCount));
        }

        private void SearchFriends(string search)
        {
            SearchFriendsAsync(search, RestartFriendsOperationsCancellationToken()).Forget();
        }

        private async UniTask SearchFriendsAsync(string search, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(search))
            {
                View.DisableSearchMode();
                searchingFriends = false;
                ShowOrHideMoreFriendsToLoadHint();
                return;
            }

            string[] friendsToAdd = await friendsController
                                         .GetFriendsAsync(search, MAX_SEARCHED_FRIENDS, cancellationToken)
                                         .Timeout(TimeSpan.FromSeconds(GET_FRIENDS_TIMEOUT));

            for (int i = 0; i < friendsToAdd.Length; i++)
                HandleFriendshipUpdated(friendsToAdd[i], FriendshipAction.APPROVED);

            View.EnableSearchMode();
            View.HideMoreFriendsToLoadHint();
            searchingFriends = true;
        }

        private void OpenFriendRequestDetails(string userId)
        {
            if (!isNewFriendRequestsEnabled) return;

            FriendRequest friendRequest = friendsController.GetAllocatedFriendRequestByUser(userId);

            if (friendRequest == null)
            {
                Debug.LogError($"Could not find an allocated friend request for user: {userId}");
                return;
            }

            if (friendRequest.IsSentTo(userId))
                dataStore.HUDs.openSentFriendRequestDetail.Set(friendRequest.FriendRequestId, true);
            else
                dataStore.HUDs.openReceivedFriendRequestDetail.Set(friendRequest.FriendRequestId, true);
        }

        private CancellationToken RestartFriendsOperationsCancellationToken()
        {
            friendOperationsCancellationToken = friendOperationsCancellationToken.SafeRestart();
            return friendOperationsCancellationToken.Token;
        }
    }
}
