using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DCL;
using SocialFeaturesAnalytics;
using UnityEngine;

public class FriendsHUDController : IHUD
{
    private const int LOAD_FRIENDS_ON_DEMAND_COUNT = 30;
    private const int MAX_SEARCHED_FRIENDS = 100;

    private readonly Dictionary<string, FriendEntryModel> friends = new Dictionary<string, FriendEntryModel>();
    private readonly Dictionary<string, FriendEntryModel> onlineFriends = new Dictionary<string, FriendEntryModel>();
    private readonly DataStore dataStore;
    private readonly IFriendsController friendsController;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly ISocialAnalytics socialAnalytics;
    private readonly IChatController chatController;
    private readonly IMouseCatcher mouseCatcher;
    private BaseVariable<HashSet<string>> visibleTaskbarPanels => dataStore.HUDs.visibleTaskbarPanels;

    private UserProfile ownUserProfile;
    private bool searchingFriends;
    private int lastSkipForFriends;
    private int lastSkipForFriendRequests;

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

    public void Initialize(IFriendsHUDComponentView view = null)
    {
        view ??= FriendsHUDComponentView.Create();
        View = view;

        view.Initialize(chatController, friendsController, socialAnalytics);
        view.RefreshFriendsTab();
        view.OnFriendRequestApproved += HandleRequestAccepted;
        view.OnCancelConfirmation += HandleRequestCancelled;
        view.OnRejectConfirmation += HandleRequestRejected;
        view.OnFriendRequestSent += HandleRequestSent;
        view.OnWhisper += HandleOpenWhisperChat;
        view.OnClose += HandleViewClosed;
        view.OnRequireMoreFriends += DisplayMoreFriends;
        view.OnRequireMoreFriendRequests += DisplayMoreFriendRequests;
        view.OnSearchFriendsRequested += SearchFriends;
        view.OnFriendListDisplayed += DisplayFriendsIfAnyIsLoaded;
        view.OnRequestListDisplayed += DisplayFriendRequestsIfAnyIsLoaded;

        if(mouseCatcher != null)
            mouseCatcher.OnMouseLock += HandleViewClosed;

        ownUserProfile = userProfileBridge.GetOwn();
        ownUserProfile.OnUpdate -= HandleProfileUpdated;
        ownUserProfile.OnUpdate += HandleProfileUpdated;

        if (friendsController != null)
        {
            friendsController.OnUpdateFriendship += HandleFriendshipUpdated;
            friendsController.OnUpdateUserStatus += HandleUserStatusUpdated;
            friendsController.OnFriendNotFound += OnFriendNotFound;
            
            if (friendsController.IsInitialized)
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
            View.OnFriendListDisplayed -= DisplayFriendsIfAnyIsLoaded;
            View.OnRequestListDisplayed -= DisplayFriendRequestsIfAnyIsLoaded;
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
                DisplayMoreFriends();
            else if (View.IsRequestListActive && lastSkipForFriendRequests <= 0)
                DisplayMoreFriendRequests();
        }
        
        UpdateNotificationsCounter();
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
            case FriendshipStatus.REQUESTED_TO:
                var sentRequest = friends.ContainsKey(userId)
                    ? new FriendRequestEntryModel(friends[userId], false)
                    : new FriendRequestEntryModel {isReceived = false};
                sentRequest.CopyFrom(status);
                sentRequest.blocked = IsUserBlocked(userId);
                friends[userId] = sentRequest;
                onlineFriends.Remove(userId);
                View.Set(userId, sentRequest);
                break;
            case FriendshipStatus.REQUESTED_FROM:
                var receivedRequest = friends.ContainsKey(userId)
                    ? new FriendRequestEntryModel(friends[userId], true)
                    : new FriendRequestEntryModel {isReceived = true};
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
        var userProfile = userProfileBridge.Get(userId);

        if (userProfile == null)
        {
            Debug.LogError($"UserProfile is null for {userId}! ... friendshipAction {friendshipAction}");
            return;
        }

        userProfile.OnUpdate -= HandleFriendProfileUpdated;

        switch (friendshipAction)
        {
            case FriendshipAction.NONE:
            case FriendshipAction.REJECTED:
            case FriendshipAction.CANCELLED:
            case FriendshipAction.DELETED:
                friends.Remove(userId);
                View.Remove(userId);
                onlineFriends.Remove(userId);
                break;
            case FriendshipAction.APPROVED:
                var approved = friends.ContainsKey(userId)
                    ? new FriendEntryModel(friends[userId])
                    : new FriendEntryModel();
                approved.CopyFrom(userProfile);
                approved.blocked = IsUserBlocked(userId);
                friends[userId] = approved;
                View.Set(userId, approved);
                userProfile.OnUpdate += HandleFriendProfileUpdated;
                break;
            case FriendshipAction.REQUESTED_FROM:
                var requestReceived = friends.ContainsKey(userId)
                    ? new FriendRequestEntryModel(friends[userId], true)
                    : new FriendRequestEntryModel {isReceived = true};
                requestReceived.CopyFrom(userProfile);
                requestReceived.blocked = IsUserBlocked(userId);
                friends[userId] = requestReceived;
                View.Set(userId, requestReceived);
                userProfile.OnUpdate += HandleFriendProfileUpdated;
                break;
            case FriendshipAction.REQUESTED_TO:
                var requestSent = friends.ContainsKey(userId)
                    ? new FriendRequestEntryModel(friends[userId], false)
                    : new FriendRequestEntryModel {isReceived = false};
                requestSent.CopyFrom(userProfile);
                requestSent.blocked = IsUserBlocked(userId);
                friends[userId] = requestSent;
                View.Set(userId, requestSent);
                userProfile.OnUpdate += HandleFriendProfileUpdated;
                break;
        }

        UpdateNotificationsCounter();
        ShowOrHideMoreFriendsToLoadHint();
        ShowOrHideMoreFriendRequestsToLoadHint();
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
    
    private void DisplayFriendsIfAnyIsLoaded()
    {
        if (View.FriendCount > 0) return;
        if (lastSkipForFriends > 0) return;
        DisplayMoreFriends();
    }

    private void DisplayMoreFriends()
    {
        if (!friendsController.IsInitialized) return;
        
        friendsController.GetFriends(LOAD_FRIENDS_ON_DEMAND_COUNT, lastSkipForFriends);

        // We are not handling properly the case when the friends are not fetched correctly from server.
        // 'lastSkipForFriends' will have an invalid value.
        lastSkipForFriends += LOAD_FRIENDS_ON_DEMAND_COUNT;
        
        ShowOrHideMoreFriendsToLoadHint();
    }

    private void DisplayMoreFriendRequests()
    {
        if (!friendsController.IsInitialized) return;
        if (searchingFriends) return;
        
        friendsController.GetFriendRequests(
            LOAD_FRIENDS_ON_DEMAND_COUNT, lastSkipForFriendRequests,
            LOAD_FRIENDS_ON_DEMAND_COUNT, lastSkipForFriendRequests);
        
        // We are not handling properly the case when the friend requests are not fetched correctly from server.
        // 'lastSkipForFriendRequests' will have an invalid value.
        lastSkipForFriendRequests += LOAD_FRIENDS_ON_DEMAND_COUNT;
        
        ShowOrHideMoreFriendRequestsToLoadHint();
    }
    
    private void DisplayFriendRequestsIfAnyIsLoaded()
    {
        if (View.FriendRequestCount > 0) return;
        if (lastSkipForFriendRequests > 0) return;
        DisplayMoreFriendRequests();
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
        if (string.IsNullOrEmpty(search))
        {
            View.DisableSearchMode();
            searchingFriends = false;
            ShowOrHideMoreFriendsToLoadHint();
            return;
        }

        friendsController.GetFriends(search, MAX_SEARCHED_FRIENDS);

        View.EnableSearchMode();
        View.HideMoreFriendsToLoadHint();
        searchingFriends = true;
    }
}