using System;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Helpers;
using DCL.Interface;
using UnityEngine;

public class FriendsHUDController : IHUD
{
    internal const string PLAYER_PREFS_SEEN_FRIEND_COUNT = "SeenFriendsCount";
    public IFriendsHUDComponentView view { get; private set; }

    private readonly Dictionary<string, FriendEntryBase.Model> friends = new Dictionary<string, FriendEntryBase.Model>();
    private IFriendsController friendsController;
    private UserProfile ownUserProfile;
    
    public event Action<string> OnPressWhisper;
    public event Action OnFriendsOpened;
    public event Action OnFriendsClosed;

    public void Initialize(IFriendsController friendsController, UserProfile ownUserProfile, IChatController chatController)
    {
        if (DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("social_bar_v1"))
            view = FriendsHUDComponentView.Create();
        else
            view = FriendsHUDView.Create(this);
            
        this.friendsController = friendsController;

        if (this.friendsController != null)
        {
            this.friendsController.OnUpdateFriendship += OnUpdateFriendship;
            this.friendsController.OnUpdateUserStatus += OnUpdateUserStatus;
            this.friendsController.OnFriendNotFound += OnFriendNotFound;
        }
        
        view.OnFriendRequestApproved += Entry_OnRequestAccepted;
        view.OnCancelConfirmation += Entry_OnRequestCancelled;
        view.OnRejectConfirmation += Entry_OnRequestRejected;
        view.OnFriendRequestSent += Entry_OnRequestSent;
        view.OnWhisper += Entry_OnWhisper;
        view.OnDeleteConfirmation += Entry_OnDelete;
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
                friendsController.OnInitialized -= FriendsController_OnInitialized;
                friendsController.OnInitialized += FriendsController_OnInitialized;
            }
        }
        
        chatController.OnAddMessage += HandleChatMessageAdded;
    }

    private void HandleViewClosed() => SetVisibility(false);

    private void FriendsController_OnInitialized()
    {
        friendsController.OnInitialized -= FriendsController_OnInitialized;
        view.HideSpinner();
    }

    private void OnUserProfileUpdate(UserProfile profile)
    {
        //NOTE(Brian): HashSet to check Contains quicker.
        HashSet<string> allBlockedUsers;

        if (profile.blocked != null)
            allBlockedUsers = new HashSet<string>(profile.blocked);
        else
            allBlockedUsers = new HashSet<string>();

        var entries = view.GetAllEntries();
        int entriesCount = entries.Count;

        for (int i = 0; i < entriesCount; i++)
        {
            entries[i].model.blocked = allBlockedUsers.Contains(entries[i].userId);
            entries[i].Populate(entries[i].model);
        }
    }

    private void Entry_OnRequestSent(string userNameOrId)
    {
        if (AreAlreadyFriends(userNameOrId))
        {
            view.ShowRequestSendError(FriendRequestError.AlreadyFriends);
        }
        else
        {
            WebInterface.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage
            {
                userId = userNameOrId,
                action = FriendshipAction.REQUESTED_TO
            });

            view.ShowRequestSendSuccess();
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

        view.UpdateEntry(userId, model);
    }

    void OnFriendNotFound(string name) { view.DisplayFriendUserNotFound(); }

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
        view.UpdateFriendshipStatus(userId, friendshipAction, friendEntryModel);
        UpdateNotificationsCounter();
    }

    private void UpdateNotificationsCounter()
    {
        //NOTE(Brian): If friends tab is already active, update and save this value instantly
        if (view.IsFriendListFocused())
        {
            PlayerPrefsUtils.SetInt(PLAYER_PREFS_SEEN_FRIEND_COUNT, friendsController.friendCount);
            PlayerPrefsUtils.Save();
        }

        var pendingFriendRequestsSO = NotificationScriptableObjects.pendingFriendRequests;
        int receivedRequestsCount = view.GetReceivedFriendRequestCount();

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

    private void Entry_OnWhisper(FriendEntry entry) { OnPressWhisper?.Invoke(entry.userId); }

    private void Entry_OnDelete(string userId)
    {
        WebInterface.UpdateFriendshipStatus(
            new FriendsController.FriendshipUpdateStatusMessage()
            {
                action = FriendshipAction.DELETED,
                userId = userId
            });
    }

    private void Entry_OnRequestRejected(FriendRequestEntry entry)
    {
        WebInterface.UpdateFriendshipStatus(
            new FriendsController.FriendshipUpdateStatusMessage()
            {
                action = FriendshipAction.REJECTED,
                userId = entry.userId
            });
    }

    private void Entry_OnRequestCancelled(FriendRequestEntry entry)
    {
        WebInterface.UpdateFriendshipStatus(
            new FriendsController.FriendshipUpdateStatusMessage()
            {
                action = FriendshipAction.CANCELLED,
                userId = entry.userId
            });
    }

    private void Entry_OnRequestAccepted(FriendRequestEntry entry)
    {
        WebInterface.UpdateFriendshipStatus(
            new FriendsController.FriendshipUpdateStatusMessage()
            {
                action = FriendshipAction.APPROVED,
                userId = entry.userId
            });
    }

    public void Dispose()
    {
        if (friendsController != null)
        {
            friendsController.OnInitialized -= FriendsController_OnInitialized;
            friendsController.OnUpdateFriendship -= OnUpdateFriendship;
            friendsController.OnUpdateUserStatus -= OnUpdateUserStatus;
        }

        if (view != null)
            view.OnClose -= HandleViewClosed;
        view?.Destroy();

        if (ownUserProfile != null)
            ownUserProfile.OnUpdate -= OnUserProfileUpdate;
    }

    public void SetVisibility(bool visible)
    {
        if (visible)
            view.Show();
        else
            view.Hide();

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
    
    private void HandleChatMessageAdded(ChatMessage message)
    {
        if (message.messageType != ChatMessage.Type.PRIVATE) return;

        var friendId = message.sender != ownUserProfile.userId
            ? message.sender
            : message.recipient;
        if (!friends.ContainsKey(friendId)) return;
        
        var friend = friends[friendId];
        view.SortEntriesByTimestamp(friend, message.timestamp);
    }
}