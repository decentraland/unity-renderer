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

    private readonly Dictionary<string, FriendEntryBase.Model> friends = new Dictionary<string, FriendEntryBase.Model>();
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
            entries[i].model.blocked = allBlockedUsers.Contains(entries[i].model.userId);
            entries[i].Populate(entries[i].model);
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
                socialAnalytics.SendFriendRequestSent(ownUserProfile.userId, userNameOrId, 0, PlayerActionSource.FriendsHUD);

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
        if (!friends.ContainsKey(userId))
            friends[userId] = new FriendEntryBase.Model();
        
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

        View.UpdateEntry(userId, model);
    }

    void OnFriendNotFound(string name) { View.DisplayFriendUserNotFound(); }

    private void OnUpdateFriendship(string userId, FriendshipAction friendshipAction)
    {
        UserProfile userProfile = UserProfileController.userProfilesCatalog.Get(userId);

        if (userProfile == null)
        {
            Debug.LogError($"UserProfile is null for {userId}! ... friendshipAction {friendshipAction}");
            return;
        }

        if (!friends.ContainsKey(userId))
            friends[userId] = new FriendEntryBase.Model();
        
        var friendEntryModel = friends[userId];
        friendEntryModel.userId = userId;
        friendEntryModel.userName = userProfile.userName;
        friendEntryModel.avatarSnapshotObserver = userProfile.snapshotObserver;

        if (ownUserProfile != null && ownUserProfile.blocked != null)
            friendEntryModel.blocked = ownUserProfile.blocked.Contains(userId);
        View.UpdateFriendshipStatus(userId, friendshipAction, friendEntryModel);
        UpdateNotificationsCounter();
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

    private void HandleOpenWhisperChat(FriendEntry entry) { OnPressWhisper?.Invoke(entry.model.userId); }

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
                userId = entry.model.userId
            });
        
        UpdateNotificationsCounter();

        if (ownUserProfile != null)
            socialAnalytics.SendFriendRequestRejected(ownUserProfile.userId, entry.model.userId, PlayerActionSource.FriendsHUD);
    }

    private void HandleRequestCancelled(FriendRequestEntry entry)
    {
        WebInterface.UpdateFriendshipStatus(
            new FriendsController.FriendshipUpdateStatusMessage()
            {
                action = FriendshipAction.CANCELLED,
                userId = entry.model.userId
            });

        if (ownUserProfile != null)
            socialAnalytics.SendFriendRequestCancelled(ownUserProfile.userId, entry.model.userId, PlayerActionSource.FriendsHUD);
    }

    private void HandleRequestAccepted(FriendRequestEntry entry)
    {
        WebInterface.UpdateFriendshipStatus(
            new FriendsController.FriendshipUpdateStatusMessage()
            {
                action = FriendshipAction.APPROVED,
                userId = entry.model.userId
            });

        if(ownUserProfile != null)
            socialAnalytics.SendFriendRequestApproved(ownUserProfile.userId, entry.model.userId, PlayerActionSource.FriendsHUD);
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
            View.OnClose -= HandleViewClosed;
        View?.Destroy();

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