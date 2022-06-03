using DCL.Helpers;
using DCL.Interface;
using SocialFeaturesAnalytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DCL;
using UnityEngine;

public class FriendsHUDController : IHUD
{
    private const string PLAYER_PREFS_SEEN_FRIEND_COUNT = "SeenFriendsCount";
    private const int INITIAL_DISPLAYED_FRIEND_COUNT = 50;
    private const int LOAD_FRIENDS_ON_DEMAND_COUNT = 30;
    private const int MAX_SEARCHED_FRIENDS = 100;

    private readonly Dictionary<string, FriendEntryModel> friends = new Dictionary<string, FriendEntryModel>();
    private readonly Queue<string> pendingFriends = new Queue<string>();
    private readonly Queue<string> pendingRequests = new Queue<string>();
    private readonly DataStore dataStore;

    private IFriendsController friendsController;
    private ISocialAnalytics socialAnalytics;
    private UserProfile ownUserProfile;

    public IFriendsHUDComponentView View { get; private set; }

    public event Action<string> OnPressWhisper;
    public event Action OnFriendsOpened;
    public event Action OnFriendsClosed;

    public FriendsHUDController(DataStore dataStore)
    {
        this.dataStore = dataStore;
    }

    // TODO: refactor into dependency injection, solve static usages & define better responsibilities controller<->view
    public void Initialize(
        IFriendsController friendsController,
        UserProfile ownUserProfile,
        ISocialAnalytics socialAnalytics,
        IFriendsHUDComponentView view = null)
    {
        view ??= FriendsHUDComponentView.Create();
        View = view;
        this.friendsController = friendsController;
        this.socialAnalytics = socialAnalytics;

        if (this.friendsController != null)
        {
            this.friendsController.OnUpdateFriendship += OnUpdateFriendship;
            this.friendsController.OnUpdateUserStatus += OnUpdateUserStatus;
            this.friendsController.OnFriendNotFound += OnFriendNotFound;
        }

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

        if (ownUserProfile != null)
        {
            this.ownUserProfile = ownUserProfile;
            ownUserProfile.OnUpdate += OnUserProfileUpdate;
        }

        if (friendsController != null)
        {
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

    private void HandleViewClosed() => SetVisibility(false);

    private void HandleFriendsInitialized()
    {
        friendsController.OnInitialized -= HandleFriendsInitialized;
        View.HideSpinner();
    }

    private void OnUserProfileUpdate(UserProfile profile)
    {
        //NOTE(Brian): HashSet to check Contains quicker.
        HashSet<string> allBlockedUsers;

        if (profile.blocked != null)
            allBlockedUsers = new HashSet<string>(profile.blocked);
        else
            allBlockedUsers = new HashSet<string>();

        var entries = View.GetAllEntries();
        int entriesCount = entries.Count;

        for (int i = 0; i < entriesCount; i++)
        {
            entries[i].Model.blocked = allBlockedUsers.Contains(entries[i].Model.userId);
            entries[i].Populate(entries[i].Model);
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
            WebInterface.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage
            {
                userId = userNameOrId,
                action = FriendshipAction.REQUESTED_TO
            });

            if (ownUserProfile != null)
                socialAnalytics.SendFriendRequestSent(ownUserProfile.userId, userNameOrId, 0,
                    PlayerActionSource.FriendsHUD);

            View.ShowRequestSendSuccess();
        }
    }

    private bool AreAlreadyFriends(string userNameOrId)
    {
        var userId = userNameOrId;
        var profile = UserProfileController.userProfilesCatalog.GetValues()
            .FirstOrDefault(p => p.userName == userNameOrId);

        if (profile != default)
            userId = profile.userId;

        return friendsController != null
               && friendsController.ContainsStatus(userId, FriendshipStatus.FRIEND);
    }

    private void OnUpdateUserStatus(string userId, FriendsController.UserStatus newStatus)
    {
        var shouldDisplay = true;

        if (!friends.ContainsKey(userId))
        {
            friends[userId] = new FriendEntryModel();

            if (newStatus.presence != PresenceStatus.ONLINE)
            {
                switch (newStatus.friendshipStatus)
                {
                    case FriendshipStatus.FRIEND:
                        shouldDisplay = View.FriendCount < INITIAL_DISPLAYED_FRIEND_COUNT;
                        break;
                    case FriendshipStatus.REQUESTED_FROM:
                        shouldDisplay = View.FriendRequestCount < INITIAL_DISPLAYED_FRIEND_COUNT;
                        break;
                }
            }
        }

        var model = friends[userId];
        model.userId = userId;
        model.status = newStatus.presence;
        model.coords = newStatus.position;

        if (newStatus.realm != null)
        {
            model.realm = $"{newStatus.realm.serverName.ToUpperFirst()} {newStatus.realm.layer.ToUpperFirst()}";
            model.realmServerName = newStatus.realm.serverName;
            model.realmLayerName = newStatus.realm.layer;
        }
        else
        {
            model.realm = string.Empty;
            model.realmServerName = string.Empty;
            model.realmLayerName = string.Empty;
        }

        if (shouldDisplay)
        {
            View.Set(userId, newStatus.friendshipStatus, model);
            UpdateNotificationsCounter();
        }
        else
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
    }

    private void OnUpdateFriendship(string userId, FriendshipAction friendshipAction)
    {
        var userProfile = UserProfileController.userProfilesCatalog.Get(userId);

        if (userProfile == null)
        {
            Debug.LogError($"UserProfile is null for {userId}! ... friendshipAction {friendshipAction}");
            return;
        }

        var shouldDisplay = true;

        if (!friends.ContainsKey(userId))
        {
            friends[userId] = new FriendEntryModel();

            switch (friendshipAction)
            {
                case FriendshipAction.APPROVED:
                    shouldDisplay = View.FriendCount <= INITIAL_DISPLAYED_FRIEND_COUNT;
                    break;
                case FriendshipAction.REQUESTED_FROM:
                    shouldDisplay = View.FriendRequestCount <= INITIAL_DISPLAYED_FRIEND_COUNT;
                    break;
            }
        }

        var friendEntryModel = friends[userId];
        friendEntryModel.userId = userId;
        friendEntryModel.userName = userProfile.userName;
        friendEntryModel.avatarSnapshotObserver = userProfile.snapshotObserver;

        if (ownUserProfile != null && ownUserProfile.blocked != null)
            friendEntryModel.blocked = ownUserProfile.blocked.Contains(userId);

        if (shouldDisplay)
        {
            View.Set(userId, friendshipAction, friendEntryModel);
            UpdateNotificationsCounter();
        }
        else
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
    }

    private void OnFriendNotFound(string name)
    {
        View.DisplayFriendUserNotFound();
    }

    private void UpdateNotificationsCounter()
    {
        //NOTE(Brian): If friends tab is already active, update and save this value instantly
        if (View.IsActive())
        {
            PlayerPrefsUtils.SetInt(PLAYER_PREFS_SEEN_FRIEND_COUNT, friendsController.friendCount);
            PlayerPrefsUtils.Save();
        }

        var pendingFriendRequestsSO = NotificationScriptableObjects.pendingFriendRequests;
        
        if (pendingFriendRequestsSO != null)
        {
            var receivedRequestsCount = friendsController.ReceivedRequestCount;
            pendingFriendRequestsSO.Set(receivedRequestsCount);
        }

        int seenFriendsCount = PlayerPrefs.GetInt(PLAYER_PREFS_SEEN_FRIEND_COUNT, 0);
        int friendsCount = friendsController.friendCount;
        int newFriends = friendsCount - seenFriendsCount;

        //NOTE(Brian): If someone deletes you, don't show badge notification
        if (newFriends < 0)
            newFriends = 0;

        var newApprovedFriendsSO = NotificationScriptableObjects.newApprovedFriends;

        if (newApprovedFriendsSO != null)
        {
            newApprovedFriendsSO.Set(newFriends);
        }
    }

    private void HandleOpenWhisperChat(FriendEntry entry)
    {
        OnPressWhisper?.Invoke(entry.Model.userId);
    }

    private void HandleUnfriend(string userId)
    {
    }

    private void HandleRequestRejected(FriendRequestEntry entry)
    {
        WebInterface.UpdateFriendshipStatus(
            new FriendsController.FriendshipUpdateStatusMessage
            {
                action = FriendshipAction.REJECTED,
                userId = entry.Model.userId
            });

        UpdateNotificationsCounter();

        if (ownUserProfile != null)
            socialAnalytics.SendFriendRequestRejected(ownUserProfile.userId, entry.Model.userId,
                PlayerActionSource.FriendsHUD);
    }

    private void HandleRequestCancelled(FriendRequestEntry entry)
    {
        WebInterface.UpdateFriendshipStatus(
            new FriendsController.FriendshipUpdateStatusMessage()
            {
                action = FriendshipAction.CANCELLED,
                userId = entry.Model.userId
            });

        if (ownUserProfile != null)
            socialAnalytics.SendFriendRequestCancelled(ownUserProfile.userId, entry.Model.userId,
                PlayerActionSource.FriendsHUD);
    }

    private void HandleRequestAccepted(FriendRequestEntry entry)
    {
        WebInterface.UpdateFriendshipStatus(
            new FriendsController.FriendshipUpdateStatusMessage
            {
                action = FriendshipAction.APPROVED,
                userId = entry.Model.userId
            });

        if (ownUserProfile != null)
            socialAnalytics.SendFriendRequestApproved(ownUserProfile.userId, entry.Model.userId,
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
            ownUserProfile.OnUpdate -= OnUserProfileUpdate;
    }

    public void SetVisibility(bool visible)
    {
        if (visible)
            View.Show();
        else
            View.Hide();

        if (visible)
        {
            UpdateNotificationsCounter();

            OnFriendsOpened?.Invoke();

            AudioScriptableObjects.dialogOpen.Play(true);
        }
        else
        {
            OnFriendsClosed?.Invoke();

            AudioScriptableObjects.dialogClose.Play(true);
        }
    }
}