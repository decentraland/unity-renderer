using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using DCL;
using SocialFeaturesAnalytics;
using UnityEngine;

public class FriendsHUDController : IHUD
{
    private const int INITIAL_DISPLAYED_FRIEND_COUNT = 50;
    private const int LOAD_FRIENDS_ON_DEMAND_COUNT = 30;
    private const int MAX_SEARCHED_FRIENDS = 100;

    private readonly Dictionary<string, FriendEntryModel> friends = new Dictionary<string, FriendEntryModel>();
    private readonly Queue<string> pendingFriends = new Queue<string>();
    private readonly Queue<string> pendingRequests = new Queue<string>();
    private readonly DataStore dataStore;
    private readonly IFriendsController friendsController;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly ISocialAnalytics socialAnalytics;
    private readonly IChatController chatController;
    private readonly ILastReadMessagesService lastReadMessagesService;

    private UserProfile ownUserProfile;

    public IFriendsHUDComponentView View { get; private set; }

    public event Action<string> OnPressWhisper;
    public event Action OnFriendsOpened;
    public event Action OnFriendsClosed;

    public FriendsHUDController(DataStore dataStore,
        IFriendsController friendsController,
        IUserProfileBridge userProfileBridge,
        ISocialAnalytics socialAnalytics,
        IChatController chatController,
        ILastReadMessagesService lastReadMessagesService)
    {
        this.dataStore = dataStore;
        this.friendsController = friendsController;
        this.userProfileBridge = userProfileBridge;
        this.socialAnalytics = socialAnalytics;
        this.chatController = chatController;
        this.lastReadMessagesService = lastReadMessagesService;
    }

    public void Initialize(IFriendsHUDComponentView view = null)
    {
        view ??= FriendsHUDComponentView.Create();
        View = view;

        view.Initialize(chatController, lastReadMessagesService, friendsController, socialAnalytics);
        view.ListByOnlineStatus = dataStore.featureFlags.flags.Get().IsFeatureEnabled("friends_by_online_status");
        view.OnFriendRequestApproved += HandleRequestAccepted;
        view.OnCancelConfirmation += HandleRequestCancelled;
        view.OnRejectConfirmation += HandleRequestRejected;
        view.OnFriendRequestSent += HandleRequestSent;
        view.OnWhisper += HandleOpenWhisperChat;
        view.OnClose += HandleViewClosed;
        view.OnRequireMoreFriends += DisplayMoreFriends;
        view.OnRequireMoreFriendRequests += DisplayMoreFriendRequests;
        view.OnSearchFriendsRequested += SearchFriends;

        ownUserProfile = userProfileBridge.GetOwn();
        ownUserProfile.OnUpdate -= HandleProfileUpdated;
        // ownUserProfile.OnUpdate += HandleProfileUpdated;

        if (friendsController != null)
        {
            friendsController.OnUpdateFriendship += HandleFriendshipUpdated;
            friendsController.OnUpdateUserStatus += HandleUserStatusUpdated;
            friendsController.OnFriendNotFound += OnFriendNotFound;

            if (friendsController.isInitialized)
            {
                view.HideLoadingSpinner();
            }
            else
            {
                view.ShowLoadingSpinner();
                friendsController.OnInitialized -= HandleFriendsInitialized;
                friendsController.OnInitialized += HandleFriendsInitialized;
            }
        }

        ShowOrHideMoreFriendsToLoadHint();
        ShowOrHideMoreFriendRequestsToLoadHint();
    }

    public void Dispose()
    {
        if (friendsController != null)
        {
            friendsController.OnInitialized -= HandleFriendsInitialized;
            friendsController.OnUpdateFriendship -= HandleFriendshipUpdated;
            friendsController.OnUpdateUserStatus -= HandleUserStatusUpdated;
        }

        if (View != null)
        {
            View.OnFriendRequestApproved -= HandleRequestAccepted;
            View.OnCancelConfirmation -= HandleRequestCancelled;
            View.OnRejectConfirmation -= HandleRequestRejected;
            View.OnFriendRequestSent -= HandleRequestSent;
            View.OnWhisper -= HandleOpenWhisperChat;
            View.OnDeleteConfirmation -= HandleUnfriend;
            View.OnClose -= HandleViewClosed;
            View.OnRequireMoreFriends -= DisplayMoreFriends;
            View.OnRequireMoreFriendRequests -= DisplayMoreFriendRequests;
            View.OnSearchFriendsRequested -= SearchFriends;
            View.Dispose();
        }

        if (ownUserProfile != null)
            ownUserProfile.OnUpdate -= HandleProfileUpdated;

        if (userProfileBridge != null)
        {
            foreach (var friendId in friends.Keys)
            {
                var profile = userProfileBridge.Get(friendId);
                if (profile == null) continue;
                profile.OnUpdate -= HandleFriendProfileUpdated;
            }
        }
    }

    public void SetVisibility(bool visible)
    {
        if (visible)
        {
            View.Show();
            UpdateNotificationsCounter();
            OnFriendsOpened?.Invoke();
        }
        else
        {
            View.Hide();
            OnFriendsClosed?.Invoke();
        }
    }

    private void HandleViewClosed() => SetVisibility(false);

    private void HandleFriendsInitialized()
    {
        friendsController.OnInitialized -= HandleFriendsInitialized;
        View.HideLoadingSpinner();
    }

    private void HandleProfileUpdated(UserProfile profile) => UpdateBlockStatus(profile).Forget();

    private async UniTask UpdateBlockStatus(UserProfile profile)
    {
        const int iterationsPerFrame = 10;

        //NOTE(Brian): HashSet to check Contains quicker.
        var allBlockedUsers = profile.blocked != null
            ? new HashSet<string>(profile.blocked)
            : new HashSet<string>();

        var iterations = 0;

        foreach (var friendPair in friends)
        {
            var friendId = friendPair.Key;
            var model = friendPair.Value;

            model.blocked = allBlockedUsers.Contains(friendId);
            await UniTask.SwitchToMainThread();
            View.UpdateBlockStatus(friendId, model.blocked);

            iterations++;
            if (iterations > 0 && iterations % iterationsPerFrame == 0)
                await UniTask.NextFrame();
        }
    }

    private void HandleRequestSent(string userNameOrId)
    {
        if (AreAlreadyFriends(userNameOrId))
        {
            View.ShowRequestSendError(FriendRequestError.AlreadyFriends);
        }
        else
        {
            friendsController.RequestFriendship(userNameOrId);

            if (ownUserProfile != null)
                socialAnalytics.SendFriendRequestSent(ownUserProfile.userId, userNameOrId, 0,
                    PlayerActionSource.FriendsHUD);

            View.ShowRequestSendSuccess();
        }
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

    private void HandleUserStatusUpdated(string userId, FriendsController.UserStatus status) =>
        UpdateUserStatus(userId, status, ShouldBeDisplayed(status));

    private void UpdateUserStatus(string userId, FriendsController.UserStatus status, bool shouldDisplay)
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
                if (shouldDisplay)
                    View.Set(userId, friend);
                break;
            case FriendshipStatus.NOT_FRIEND:
                View.Remove(userId);
                friends.Remove(userId);
                break;
            case FriendshipStatus.REQUESTED_TO:
                var sentRequest = friends.ContainsKey(userId)
                    ? new FriendRequestEntryModel(friends[userId], false)
                    : new FriendRequestEntryModel {isReceived = false};
                sentRequest.CopyFrom(status);
                sentRequest.blocked = IsUserBlocked(userId);
                friends[userId] = sentRequest;
                if (shouldDisplay)
                    View.Set(userId, sentRequest);
                break;
            case FriendshipStatus.REQUESTED_FROM:
                var receivedRequest = friends.ContainsKey(userId)
                    ? new FriendRequestEntryModel(friends[userId], true)
                    : new FriendRequestEntryModel {isReceived = true};
                receivedRequest.CopyFrom(status);
                receivedRequest.blocked = IsUserBlocked(userId);
                friends[userId] = receivedRequest;
                if (shouldDisplay)
                    View.Set(userId, receivedRequest);
                break;
        }

        if (!shouldDisplay)
            EnqueueOnPendingToLoad(userId, status);
    }

    private void HandleFriendshipUpdated(string userId, FriendshipAction friendshipAction)
    {
        var userProfile = userProfileBridge.Get(userId);

        if (userProfile == null)
        {
            Debug.LogError($"UserProfile is null for {userId}! ... friendshipAction {friendshipAction}");
            return;
        }

        userProfile.OnUpdate -= HandleFriendProfileUpdated;

        var shouldDisplay = ShouldBeDisplayed(userId, friendshipAction);

        switch (friendshipAction)
        {
            case FriendshipAction.NONE:
            case FriendshipAction.REJECTED:
            case FriendshipAction.CANCELLED:
            case FriendshipAction.DELETED:
                friends.Remove(userId);
                View.Remove(userId);
                break;
            case FriendshipAction.APPROVED:
                var approved = friends.ContainsKey(userId)
                    ? new FriendEntryModel(friends[userId])
                    : new FriendEntryModel();
                approved.CopyFrom(userProfile);
                approved.blocked = IsUserBlocked(userId);
                friends[userId] = approved;
                if (shouldDisplay)
                    View.Set(userId, approved);
                // userProfile.OnUpdate += HandleFriendProfileUpdated;
                break;
            case FriendshipAction.REQUESTED_FROM:
                var requestReceived = friends.ContainsKey(userId)
                    ? new FriendRequestEntryModel(friends[userId], true)
                    : new FriendRequestEntryModel {isReceived = true};
                requestReceived.CopyFrom(userProfile);
                requestReceived.blocked = IsUserBlocked(userId);
                friends[userId] = requestReceived;
                if (shouldDisplay)
                    View.Set(userId, requestReceived);
                // userProfile.OnUpdate += HandleFriendProfileUpdated;
                break;
            case FriendshipAction.REQUESTED_TO:
                var requestSent = friends.ContainsKey(userId)
                    ? new FriendRequestEntryModel(friends[userId], false)
                    : new FriendRequestEntryModel {isReceived = false};
                requestSent.CopyFrom(userProfile);
                requestSent.blocked = IsUserBlocked(userId);
                friends[userId] = requestSent;
                if (shouldDisplay)
                    View.Set(userId, requestSent);
                // userProfile.OnUpdate += HandleFriendProfileUpdated;
                break;
        }

        if (shouldDisplay)
            UpdateNotificationsCounter();
        else
            EnqueueOnPendingToLoad(userId, friendshipAction);
    }

    private void HandleFriendProfileUpdated(UserProfile profile)
    {
        var userId = profile.userId;
        if (!friends.ContainsKey(userId)) return;
        friends[userId].CopyFrom(profile);

        var status = friendsController.GetUserStatus(profile.userId);
        if (status == null) return;

        UpdateUserStatus(userId, status, ShouldBeDisplayed(status));
    }

    private bool IsUserBlocked(string userId)
    {
        if (ownUserProfile != null && ownUserProfile.blocked != null)
            return ownUserProfile.blocked.Contains(userId);
        return false;
    }

    private void EnqueueOnPendingToLoad(string userId, FriendsController.UserStatus newStatus)
    {
        switch (newStatus.friendshipStatus)
        {
            case FriendshipStatus.FRIEND:
                pendingFriends.Enqueue(userId);
                View.ShowMoreFriendsToLoadHint(pendingFriends.Count);
                break;
            case FriendshipStatus.REQUESTED_FROM:
                pendingRequests.Enqueue(userId);
                View.ShowMoreRequestsToLoadHint(pendingRequests.Count);
                break;
        }
    }

    private void EnqueueOnPendingToLoad(string userId, FriendshipAction friendshipAction)
    {
        switch (friendshipAction)
        {
            case FriendshipAction.APPROVED:
                pendingFriends.Enqueue(userId);
                View.ShowMoreFriendsToLoadHint(pendingFriends.Count);
                break;
            case FriendshipAction.REQUESTED_FROM:
                pendingRequests.Enqueue(userId);
                View.ShowMoreRequestsToLoadHint(pendingRequests.Count);
                break;
        }
    }

    private bool ShouldBeDisplayed(string userId, FriendshipAction friendshipAction)
    {
        return friendshipAction switch
        {
            FriendshipAction.APPROVED => View.FriendCount <= INITIAL_DISPLAYED_FRIEND_COUNT ||
                                         View.ContainsFriend(userId),
            FriendshipAction.REQUESTED_FROM => View.FriendRequestCount <= INITIAL_DISPLAYED_FRIEND_COUNT ||
                                               View.ContainsFriendRequest(userId),
            _ => true
        };
    }

    private bool ShouldBeDisplayed(FriendsController.UserStatus status)
    {
        if (status.presence == PresenceStatus.ONLINE) return true;

        return status.friendshipStatus switch
        {
            FriendshipStatus.FRIEND => View.FriendCount < INITIAL_DISPLAYED_FRIEND_COUNT ||
                                       View.ContainsFriend(status.userId),
            FriendshipStatus.REQUESTED_FROM => View.FriendRequestCount < INITIAL_DISPLAYED_FRIEND_COUNT ||
                                               View.ContainsFriendRequest(status.userId),
            _ => true
        };
    }

    private void OnFriendNotFound(string name)
    {
        View.DisplayFriendUserNotFound();
    }

    private void UpdateNotificationsCounter()
    {
        if (View.IsActive())
            dataStore.friendNotifications.seenFriends.Set(friendsController.friendCount);

        dataStore.friendNotifications.seenRequests.Set(friendsController.ReceivedRequestCount);
    }

    private void HandleOpenWhisperChat(FriendEntryModel entry) => OnPressWhisper?.Invoke(entry.userId);

    private void HandleUnfriend(string userId) => friendsController.RemoveFriend(userId);

    private void HandleRequestRejected(FriendRequestEntryModel entry)
    {
        friendsController.RejectFriendship(entry.userId);

        UpdateNotificationsCounter();

        if (ownUserProfile != null)
            socialAnalytics.SendFriendRequestRejected(ownUserProfile.userId, entry.userId,
                PlayerActionSource.FriendsHUD);
    }

    private void HandleRequestCancelled(FriendRequestEntryModel entry)
    {
        friendsController.CancelRequest(entry.userId);

        if (ownUserProfile != null)
            socialAnalytics.SendFriendRequestCancelled(ownUserProfile.userId, entry.userId,
                PlayerActionSource.FriendsHUD);
    }

    private void HandleRequestAccepted(FriendRequestEntryModel entry)
    {
        friendsController.AcceptFriendship(entry.userId);

        if (ownUserProfile != null)
            socialAnalytics.SendFriendRequestApproved(ownUserProfile.userId, entry.userId,
                PlayerActionSource.FriendsHUD);
    }

    private void DisplayMoreFriends()
    {
        for (var i = 0; i < LOAD_FRIENDS_ON_DEMAND_COUNT && pendingFriends.Count > 0; i++)
        {
            var userId = pendingFriends.Dequeue();
            var status = friendsController.GetUserStatus(userId);
            if (status == null) continue;
            UpdateUserStatus(userId, status, true);
        }

        ShowOrHideMoreFriendsToLoadHint();
    }

    private void DisplayMoreFriendRequests()
    {
        for (var i = 0; i < LOAD_FRIENDS_ON_DEMAND_COUNT && pendingRequests.Count > 0; i++)
        {
            var userId = pendingRequests.Dequeue();
            var status = friendsController.GetUserStatus(userId);
            if (status == null) continue;
            HandleUserStatusUpdated(userId, status);
        }

        ShowOrHideMoreFriendRequestsToLoadHint();
    }

    private void ShowOrHideMoreFriendRequestsToLoadHint()
    {
        if (pendingRequests.Count == 0)
            View.HideMoreRequestsToLoadHint();
        else
            View.ShowMoreRequestsToLoadHint(pendingRequests.Count);
    }

    private void ShowOrHideMoreFriendsToLoadHint()
    {
        if (pendingFriends.Count == 0)
            View.HideMoreFriendsToLoadHint();
        else
            View.ShowMoreFriendsToLoadHint(pendingFriends.Count);
    }

    private void SearchFriends(string search)
    {
        if (string.IsNullOrEmpty(search))
        {
            View.ClearFriendFilter();
            ShowOrHideMoreFriendsToLoadHint();
            return;
        }

        Dictionary<string, FriendEntryModel> FilterFriendsByUserNameAndUserId(string search)
        {
            var regex = new Regex(search, RegexOptions.IgnoreCase);

            return friends.Values.Where(model =>
            {
                var status = friendsController.GetUserStatus(model.userId);
                if (status == null) return false;
                if (status.friendshipStatus != FriendshipStatus.FRIEND) return false;
                if (regex.IsMatch(model.userId)) return true;
                return !string.IsNullOrEmpty(model.userName) && regex.IsMatch(model.userName);
            }).Take(MAX_SEARCHED_FRIENDS).ToDictionary(model => model.userId, model => model);
        }

        void DisplayMissingFriends(IEnumerable<FriendEntryModel> filteredFriends)
        {
            foreach (var model in filteredFriends)
            {
                if (View.ContainsFriend(model.userId)) return;
                var status = friendsController.GetUserStatus(model.userId);
                if (status == null) continue;
                UpdateUserStatus(model.userId, status, true);
            }
        }

        var filteredFriends = FilterFriendsByUserNameAndUserId(search);
        DisplayMissingFriends(filteredFriends.Values);
        View.FilterFriends(filteredFriends);
    }
}