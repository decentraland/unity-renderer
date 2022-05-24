using DCL.Helpers;
using DCL.Interface;
using SocialFeaturesAnalytics;
using System;
using System.Collections.Generic;
using System.Linq;
using DCL;
using UnityEngine;

public class FriendsHUDController : IHUD
{
    private const string PLAYER_PREFS_SEEN_FRIEND_COUNT = "SeenFriendsCount";
    private const int INITIAL_DISPLAYED_FRIEND_COUNT = 50;
    private const int LOAD_FRIENDS_ON_DEMAND_COUNT = 30;

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
                        pendingFriends.Enqueue(userId);
                        View.ShowMoreFriendsToLoadHint(pendingFriends.Count);
                        break;
                    case FriendshipStatus.REQUESTED_FROM:
                        shouldDisplay = View.FriendRequestCount < INITIAL_DISPLAYED_FRIEND_COUNT;
                        pendingRequests.Enqueue(userId);
                        View.ShowMoreRequestsToLoadHint(pendingRequests.Count);
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
            View.Set(userId, newStatus.friendshipStatus, model);
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
                    pendingFriends.Enqueue(userId);
                    View.ShowMoreFriendsToLoadHint(pendingFriends.Count);
                    break;
                case FriendshipAction.REQUESTED_FROM:
                    shouldDisplay = View.FriendRequestCount <= INITIAL_DISPLAYED_FRIEND_COUNT;
                    pendingRequests.Enqueue(userId);
                    View.ShowMoreRequestsToLoadHint(pendingRequests.Count);
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
        int receivedRequestsCount = View.GetReceivedFriendRequestCount();

        if (pendingFriendRequestsSO != null)
        {
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
        WebInterface.UpdateFriendshipStatus(
            new FriendsController.FriendshipUpdateStatusMessage()
            {
                action = FriendshipAction.DELETED,
                userId = userId
            });
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

        if (pendingFriends.Count == 0)
            View.HideMoreFriendsToLoadHint();
        else
            View.ShowMoreFriendsToLoadHint(pendingFriends.Count);
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