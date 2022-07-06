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
    private const int LOAD_FRIENDS_ON_DEMAND_COUNT = 30;
    private const int MAX_SEARCHED_FRIENDS = 100;

    private readonly Dictionary<string, FriendEntryModel> friends = new Dictionary<string, FriendEntryModel>();
    private readonly DataStore dataStore;
    private readonly IFriendsController friendsController;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly ISocialAnalytics socialAnalytics;
    private readonly IChatController chatController;

    private UserProfile ownUserProfile;
    private bool searchingFriends;
    private DateTimeOffset oldestReceivedRequest;
    private DateTimeOffset oldestSentRequest;

    public IFriendsHUDComponentView View { get; private set; }

    public event Action<string> OnPressWhisper;
    public event Action OnOpened;
    public event Action OnClosed;

    public FriendsHUDController(DataStore dataStore,
        IFriendsController friendsController,
        IUserProfileBridge userProfileBridge,
        ISocialAnalytics socialAnalytics,
        IChatController chatController)
    {
        this.dataStore = dataStore;
        this.friendsController = friendsController;
        this.userProfileBridge = userProfileBridge;
        this.socialAnalytics = socialAnalytics;
        this.chatController = chatController;
    }

    public void Initialize(IFriendsHUDComponentView view = null)
    {
        view ??= FriendsHUDComponentView.Create();
        View = view;

        view.Initialize(chatController, friendsController, socialAnalytics);
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
        view.OnFriendListDisplayed += DisplayFriendsIfAnyIsLoaded;
        view.OnRequestListDisplayed += DisplayFriendRequestsIfAnyIsLoaded;

        ownUserProfile = userProfileBridge.GetOwn();
        ownUserProfile.OnUpdate -= HandleProfileUpdated;
        ownUserProfile.OnUpdate += HandleProfileUpdated;

        if (friendsController != null)
        {
            friendsController.OnUpdateFriendship += OnUpdateFriendship;
            friendsController.OnUpdateUserStatus += OnUpdateUserStatus;
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

    public void Dispose()
    {
        if (friendsController != null)
        {
            friendsController.OnInitialized -= HandleFriendsInitialized;
            friendsController.OnUpdateFriendship -= OnUpdateFriendship;
            friendsController.OnUpdateUserStatus -= OnUpdateUserStatus;
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
        if (visible)
        {
            View.Show();
            UpdateNotificationsCounter();
            
            if (View.IsFriendListActive)
                DisplayMoreFriends();
            else if (View.IsRequestListActive)
                DisplayMoreFriendRequests();
            
            OnOpened?.Invoke();
        }
        else
        {
            View.Hide();
            OnClosed?.Invoke();
        }
    }

    private void HandleViewClosed() => SetVisibility(false);

    private void HandleFriendsInitialized()
    {
        friendsController.OnInitialized -= HandleFriendsInitialized;
        View.HideLoadingSpinner();

        if (View.IsActive())
        {
            if (View.IsFriendListActive)
                DisplayMoreFriends();
            else if (View.IsRequestListActive)
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
            View.Populate(friendId, model);
            
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

    private void OnUpdateUserStatus(string userId, UserStatus newStatus)
    {
        var model = GetOrCreateModel(userId, newStatus);
        model.CopyFrom(newStatus);
        
        View.Set(userId, newStatus.friendshipStatus, model);
        
        UpdatePaginationState(userId, newStatus);
        UpdateNotificationsCounter();
        ShowOrHideMoreFriendsToLoadHint();
        ShowOrHideMoreFriendRequestsToLoadHint();
    }

    private void OnUpdateFriendship(string userId, FriendshipAction friendshipAction)
    {
        var userProfile = userProfileBridge.Get(userId);

        if (userProfile == null)
        {
            Debug.LogError($"UserProfile is null for {userId}! ... friendshipAction {friendshipAction}");
            return;
        }

        userProfile.OnUpdate -= HandleFriendProfileUpdated;
        userProfile.OnUpdate += HandleFriendProfileUpdated;
        
        var model = GetOrCreateModel(userId, friendshipAction);
        model.CopyFrom(userProfile);
        model.blocked = IsUserBlocked(userId);

        View.Set(userId, friendshipAction, model);
        
        UpdatePaginationState(userId, friendshipAction);
        UpdateNotificationsCounter();
        ShowOrHideMoreFriendsToLoadHint();
        ShowOrHideMoreFriendRequestsToLoadHint();
    }

    private void UpdatePaginationState(string userId, UserStatus status)
    {
        switch (status.friendshipStatus)
        {
            case FriendshipStatus.REQUESTED_TO:
            {
                var timestamp = friendsController.GetUserStatus(userId).friendshipStartedTime;
                if (timestamp < oldestSentRequest)
                    oldestSentRequest = timestamp;
                break;
            }
            case FriendshipStatus.REQUESTED_FROM:
            {
                var timestamp = friendsController.GetUserStatus(userId).friendshipStartedTime;
                if (timestamp < oldestReceivedRequest)
                    oldestReceivedRequest = timestamp;
                break;
            }
        }
    }

    private void UpdatePaginationState(string userId, FriendshipAction friendshipAction)
    {
        switch (friendshipAction)
        {
            case FriendshipAction.REQUESTED_TO:
            {
                var timestamp = friendsController.GetUserStatus(userId).friendshipStartedTime;
                if (timestamp < oldestSentRequest)
                    oldestSentRequest = timestamp;
                break;
            }
            case FriendshipAction.REQUESTED_FROM:
            {
                var timestamp = friendsController.GetUserStatus(userId).friendshipStartedTime;
                if (timestamp < oldestReceivedRequest)
                    oldestReceivedRequest = timestamp;
                break;
            }
        }
    }

    private void HandleFriendProfileUpdated(UserProfile profile)
    {
        var userId = profile.userId;
        if (!friends.ContainsKey(userId)) return;
        
        var model = friends[userId];
        model.CopyFrom(profile);
        model.blocked = IsUserBlocked(userId);
        
        View.Populate(userId, model);
    }

    private bool IsUserBlocked(string userId)
    {
        if (ownUserProfile != null && ownUserProfile.blocked != null)
            return ownUserProfile.blocked.Contains(userId);
        return false;
    }

    private FriendEntryModel GetOrCreateModel(string userId, FriendshipAction friendshipAction)
    {
        if (!friends.ContainsKey(userId))
        {
            if (friendshipAction == FriendshipAction.REQUESTED_TO
                || friendshipAction == FriendshipAction.REQUESTED_FROM
                || friendshipAction == FriendshipAction.CANCELLED
                || friendshipAction == FriendshipAction.REJECTED)
            {
                friends[userId] = new FriendRequestEntryModel
                {
                    isReceived = friendshipAction == FriendshipAction.REQUESTED_FROM
                };
            }
            else
                friends[userId] = new FriendEntryModel();
        }
        else
        {
            if (friendshipAction == FriendshipAction.REQUESTED_TO
                || friendshipAction == FriendshipAction.REQUESTED_FROM
                || friendshipAction == FriendshipAction.CANCELLED
                || friendshipAction == FriendshipAction.REJECTED)
            {
                friends[userId] = new FriendRequestEntryModel(friends[userId],
                    friendshipAction == FriendshipAction.REQUESTED_FROM);
            }
            else
                friends[userId] = new FriendEntryModel(friends[userId]);
        }

        return friends[userId];
    }

    private FriendEntryModel GetOrCreateModel(string userId, UserStatus newStatus)
    {
        if (!friends.ContainsKey(userId))
        {
            if (newStatus.friendshipStatus == FriendshipStatus.REQUESTED_TO
                || newStatus.friendshipStatus == FriendshipStatus.REQUESTED_FROM)
            {
                friends[userId] = new FriendRequestEntryModel
                {
                    isReceived = newStatus.friendshipStatus == FriendshipStatus.REQUESTED_FROM
                };
            }
            else
                friends[userId] = new FriendEntryModel();
        }
        else
        {
            if (newStatus.friendshipStatus == FriendshipStatus.REQUESTED_TO
                || newStatus.friendshipStatus == FriendshipStatus.REQUESTED_FROM)
                friends[userId] = new FriendRequestEntryModel(friends[userId],
                    newStatus.friendshipStatus == FriendshipStatus.REQUESTED_FROM);
            else
                friends[userId] = new FriendEntryModel(friends[userId]);
        }

        return friends[userId];
    }

    private void OnFriendNotFound(string name)
    {
        View.DisplayFriendUserNotFound();
    }

    private void UpdateNotificationsCounter()
    {
        if (View.IsActive())
            dataStore.friendNotifications.seenFriends.Set(View.FriendCount);
        
        dataStore.friendNotifications.pendingFriendRequestCount.Set(friendsController.TotalFriendRequestCount);
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
        DisplayMoreFriends();
    }

    private void DisplayMoreFriends()
    {
        if (!friendsController.IsInitialized) return;
        ShowOrHideMoreFriendsToLoadHint();
        friendsController.GetFriendsAsync(LOAD_FRIENDS_ON_DEMAND_COUNT, View.FriendCount);
    }
    
    private void DisplayMoreFriendRequests()
    {
        if (!friendsController.IsInitialized) return;
        ShowOrHideMoreFriendRequestsToLoadHint();
        friendsController.GetFriendRequestsAsync(
            LOAD_FRIENDS_ON_DEMAND_COUNT, oldestSentRequest.ToUnixTimeMilliseconds(),
            LOAD_FRIENDS_ON_DEMAND_COUNT, oldestReceivedRequest.ToUnixTimeMilliseconds());
    }
    
    private void DisplayFriendRequestsIfAnyIsLoaded()
    {
        if (View.FriendRequestCount > 0) return;
        DisplayMoreFriendRequests();
    }

    private void ShowOrHideMoreFriendRequestsToLoadHint()
    {
        if (View.FriendRequestCount >= friendsController.TotalFriendRequestCount)
            View.HideMoreRequestsToLoadHint();
        else
            View.ShowMoreRequestsToLoadHint(friendsController.TotalFriendRequestCount - View.FriendRequestCount);
    }

    private void ShowOrHideMoreFriendsToLoadHint()
    {
        if (View.FriendCount >= friendsController.TotalFriendCount || searchingFriends)
            View.HideMoreFriendsToLoadHint();
        else
            View.ShowMoreFriendsToLoadHint(friendsController.TotalFriendCount - View.FriendCount);
    }

    private void SearchFriends(string search)
    {
        if (string.IsNullOrEmpty(search))
        {
            View.ClearFriendFilter();
            searchingFriends = false;
            ShowOrHideMoreFriendsToLoadHint();
            return;
        }

        friendsController.GetFriendsAsync(search, MAX_SEARCHED_FRIENDS);

        Dictionary<string, FriendEntryModel> FilterFriendsByNameOrId(string search)
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

        View.FilterFriends(FilterFriendsByNameOrId(search));
        View.HideMoreFriendsToLoadHint();
        searchingFriends = true;
    }
}