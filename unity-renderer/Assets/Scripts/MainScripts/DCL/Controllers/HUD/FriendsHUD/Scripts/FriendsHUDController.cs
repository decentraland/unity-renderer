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
    private readonly IFriendsNotificationService friendsNotificationService;

    private UserProfile ownUserProfile;

    public IFriendsHUDComponentView View { get; private set; }

    public event Action<string> OnPressWhisper;
    public event Action OnFriendsOpened;
    public event Action OnFriendsClosed;

    public FriendsHUDController(DataStore dataStore,
        IFriendsController friendsController,
        IUserProfileBridge userProfileBridge,
        ISocialAnalytics socialAnalytics,
        IFriendsNotificationService friendsNotificationService)
    {
        this.dataStore = dataStore;
        this.friendsController = friendsController;
        this.userProfileBridge = userProfileBridge;
        this.socialAnalytics = socialAnalytics;
        this.friendsNotificationService = friendsNotificationService;
    }

    public void Initialize(IFriendsHUDComponentView view)
    {
        View = view;

        view.ListByOnlineStatus = dataStore.featureFlags.flags.Get().IsFeatureEnabled("friends_by_online_status");
        view.OnFriendRequestApproved += HandleRequestAccepted;
        view.OnCancelConfirmation += HandleRequestCancelled;
        view.OnRejectConfirmation += HandleRequestRejected;
        view.OnFriendRequestSent += HandleRequestSent;
        view.OnWhisper += HandleOpenWhisperChat;
        view.OnDeleteConfirmation += HandleUnfriend;
        view.OnClose += HandleViewClosed;
        view.OnRequireMoreFriends += DisplayMoreFriends;
        view.OnRequireMoreFriendRequests += DisplayMoreFriendRequests;
        view.OnSearchFriendsRequested += SearchFriends;

        ownUserProfile = userProfileBridge.GetOwn();
        ownUserProfile.OnUpdate += HandleProfileUpdated;

        if (friendsController != null)
        {
            friendsController.OnUpdateFriendship += OnUpdateFriendship;
            friendsController.OnUpdateUserStatus += OnUpdateUserStatus;
            friendsController.OnFriendNotFound += OnFriendNotFound;
            
            if (friendsController.isInitialized)
            {
                view.HideSpinner();
            }
            else
            {
                view.ShowSpinner();
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
            View.Destroy();
        }

        if (ownUserProfile != null)
            ownUserProfile.OnUpdate -= HandleProfileUpdated;
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
        View.HideSpinner();
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

    private void OnUpdateUserStatus(string userId, FriendsController.UserStatus newStatus)
    {
        var shouldDisplay = ShouldBeDisplayed(newStatus);
        var model = GetOrCreateModel(userId, newStatus);
        model.CopyFrom(newStatus);

        if (shouldDisplay)
            View.Set(userId, newStatus.friendshipStatus, model);
        else
            EnqueueOnPendingToLoad(userId, newStatus);
    }

    private void OnUpdateFriendship(string userId, FriendshipAction friendshipAction)
    {
        var userProfile = userProfileBridge.Get(userId);

        if (userProfile == null)
        {
            Debug.LogError($"UserProfile is null for {userId}! ... friendshipAction {friendshipAction}");
            return;
        }

        var shouldDisplay = ShouldBeDisplayed(userId, friendshipAction);
        var model = GetOrCreateModel(userId, friendshipAction);
        model.CopyFrom(userProfile);

        if (ownUserProfile != null && ownUserProfile.blocked != null)
            model.blocked = ownUserProfile.blocked.Contains(userId);

        if (shouldDisplay)
        {
            View.Set(userId, friendshipAction, model);
            UpdateNotificationsCounter();
        }
        else
            EnqueueOnPendingToLoad(userId, friendshipAction);
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

    private FriendEntryModel GetOrCreateModel(string userId, FriendsController.UserStatus newStatus)
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
            FriendshipAction.APPROVED => View.FriendCount <= INITIAL_DISPLAYED_FRIEND_COUNT || View.ContainsFriend(userId),
            FriendshipAction.REQUESTED_FROM => View.FriendRequestCount <= INITIAL_DISPLAYED_FRIEND_COUNT || View.ContainsFriendRequest(userId),
            _ => true
        };
    }

    private bool ShouldBeDisplayed(FriendsController.UserStatus status)
    {
        if (status.presence == PresenceStatus.ONLINE) return true;
        
        return status.friendshipStatus switch
        {
            FriendshipStatus.FRIEND => View.FriendCount < INITIAL_DISPLAYED_FRIEND_COUNT || View.ContainsFriend(status.userId),
            FriendshipStatus.REQUESTED_FROM => View.FriendRequestCount < INITIAL_DISPLAYED_FRIEND_COUNT || View.ContainsFriendRequest(status.userId),
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
            friendsNotificationService.MarkFriendsAsSeen(friendsController.friendCount);

        friendsNotificationService.MarkRequestsAsSeen(friendsController.ReceivedRequestCount);
        friendsNotificationService.UpdateUnseenFriends();
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
            if (!friends.ContainsKey(userId)) continue;
            var model = friends[userId];
            var status = friendsController.GetUserStatus(userId);
            if (status == null) continue;
            View.Set(userId, status.friendshipStatus, model);
        }

        ShowOrHideMoreFriendsToLoadHint();
    }
    
    private void DisplayMoreFriendRequests()
    {
        for (var i = 0; i < LOAD_FRIENDS_ON_DEMAND_COUNT && pendingRequests.Count > 0; i++)
        {
            var userId = pendingRequests.Dequeue();
            if (!friends.ContainsKey(userId)) continue;
            var model = friends[userId];
            var status = friendsController.GetUserStatus(userId);
            if (status == null) continue;
            View.Set(userId, status.friendshipStatus, model);
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
                View.Set(model.userId, FriendshipStatus.FRIEND, model);
            }
        }

        var filteredFriends = FilterFriendsByUserNameAndUserId(search);
        DisplayMissingFriends(filteredFriends.Values);
        View.FilterFriends(filteredFriends);
    }
}