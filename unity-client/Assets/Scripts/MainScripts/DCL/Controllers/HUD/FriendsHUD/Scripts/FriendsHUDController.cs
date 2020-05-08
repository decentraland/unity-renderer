using DCL.Helpers;
using DCL.Interface;
using System.Collections.Generic;
using UnityEngine;

public class FriendsHUDController : IHUD
{
    internal const string CURRENT_PLAYER_ID = "CurrentPlayerInfoCardId";
    public FriendsHUDView view
    {
        get;
        private set;
    }

    IFriendsController friendsController;
    public event System.Action<string> OnPressWhisper;
    InputAction_Trigger toggleTrigger;

    UserProfile ownUserProfile;

    public void Initialize(IFriendsController friendsController, UserProfile ownUserProfile)
    {
        view = FriendsHUDView.Create();
        this.friendsController = friendsController;

        if (this.friendsController != null)
        {
            this.friendsController.OnUpdateFriendship += OnUpdateFriendship;
            this.friendsController.OnUpdateUserStatus += OnUpdateUserStatus;
            this.friendsController.OnFriendNotFound += OnFriendNotFound;
        }

        view.friendRequestsList.OnFriendRequestApproved += Entry_OnRequestAccepted;
        view.friendRequestsList.OnCancelConfirmation += Entry_OnRequestCancelled;
        view.friendRequestsList.OnRejectConfirmation += Entry_OnRequestRejected;
        view.friendRequestsList.OnFriendRequestSent += Entry_OnRequestSent;
        view.friendRequestsList.contextMenuPanel.OnBlock += Entry_OnBlock;
        view.friendRequestsList.contextMenuPanel.OnPassport += Entry_OnPassport;

        view.friendsList.OnJumpIn += Entry_OnJumpIn;
        view.friendsList.OnWhisper += Entry_OnWhisper;
        view.friendsList.contextMenuPanel.OnBlock += Entry_OnBlock;
        view.friendsList.contextMenuPanel.OnPassport += Entry_OnPassport;
        view.friendsList.contextMenuPanel.OnReport += Entry_OnReport;

        view.friendsList.OnDeleteConfirmation += Entry_OnDelete;

        toggleTrigger = Resources.Load<InputAction_Trigger>("ToggleFriends");
        toggleTrigger.OnTriggered += OnHotkeyPress;

        if (ownUserProfile != null)
        {
            this.ownUserProfile = ownUserProfile;
            ownUserProfile.OnUpdate += OnUserProfileUpdate;
        }
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

    private void OnHotkeyPress(DCLAction_Trigger action)
    {
        if (action != DCLAction_Trigger.ToggleFriends)
            return;

        SetVisibility(!view.gameObject.activeSelf);
    }

    private void Entry_OnRequestSent(string userId)
    {
        WebInterface.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage() { userId = userId, action = FriendsController.FriendshipAction.REQUESTED_TO });
    }

    private void OnUpdateUserStatus(string userId, FriendsController.UserStatus newStatus)
    {
        var model = new FriendEntry.Model();

        FriendEntryBase entry = view.friendsList.GetEntry(userId) ?? view.friendRequestsList.GetEntry(userId);

        if (entry != null)
            model = entry.model;

        model.status = newStatus.presence;
        model.coords = newStatus.position;

        if(newStatus.realm != null)
            model.realm = $"{newStatus.realm.serverName.ToUpperFirst()} {newStatus.realm.layer.ToUpperFirst()}";
        else
            model.realm = string.Empty;

        view.friendsList.UpdateEntry(userId, model);
        view.friendRequestsList.UpdateEntry(userId, model);
    }

    void OnFriendNotFound(string name)
    {
        view.friendRequestsList.DisplayFriendUserNotFound();
    }

    private void OnUpdateFriendship(string userId, FriendsController.FriendshipAction friendshipAction)
    {
        var userProfile = UserProfileController.userProfilesCatalog.Get(userId);

        if (userProfile == null)
        {
            Debug.LogError($"UserProfile is null for {userId}! ... friendshipAction {friendshipAction}");
            return;
        }

        var friendEntryModel = new FriendEntry.Model();

        FriendEntryBase entry = view.friendsList.GetEntry(userId) ?? view.friendRequestsList.GetEntry(userId);

        if (entry != null)
            friendEntryModel = entry.model;

        friendEntryModel.userName = userProfile.userName;
        friendEntryModel.avatarImage = userProfile.faceSnapshot;

        userProfile.OnFaceSnapshotReadyEvent -= friendEntryModel.OnSpriteUpdate;
        userProfile.OnFaceSnapshotReadyEvent += friendEntryModel.OnSpriteUpdate;

        if (ownUserProfile != null && ownUserProfile.blocked != null)
            friendEntryModel.blocked = ownUserProfile.blocked.Contains(userId);

        switch (friendshipAction)
        {
            case FriendsController.FriendshipAction.NONE:
                userProfile.OnFaceSnapshotReadyEvent -= friendEntryModel.OnSpriteUpdate;
                view.friendRequestsList.RemoveEntry(userId);
                view.friendsList.RemoveEntry(userId);
                break;
            case FriendsController.FriendshipAction.APPROVED:
                view.friendRequestsList.RemoveEntry(userId);
                view.friendsList.CreateOrUpdateEntry(userId, friendEntryModel);
                break;
            case FriendsController.FriendshipAction.REJECTED:
                userProfile.OnFaceSnapshotReadyEvent -= friendEntryModel.OnSpriteUpdate;
                view.friendRequestsList.RemoveEntry(userId);
                break;
            case FriendsController.FriendshipAction.CANCELLED:
                userProfile.OnFaceSnapshotReadyEvent -= friendEntryModel.OnSpriteUpdate;
                view.friendRequestsList.RemoveEntry(userId);
                break;
            case FriendsController.FriendshipAction.REQUESTED_FROM:
                view.friendRequestsList.CreateOrUpdateEntry(userId, friendEntryModel, true);
                break;
            case FriendsController.FriendshipAction.REQUESTED_TO:
                view.friendRequestsList.CreateOrUpdateEntry(userId, friendEntryModel, false);
                break;
            case FriendsController.FriendshipAction.DELETED:
                userProfile.OnFaceSnapshotReadyEvent -= friendEntryModel.OnSpriteUpdate;
                view.friendRequestsList.RemoveEntry(userId);
                view.friendsList.RemoveEntry(userId);
                break;
        }

        var pendingFriendRequestsSO = Resources.Load<FloatVariable>("ScriptableObjects/PendingFriendRequests");

        if (pendingFriendRequestsSO != null)
            pendingFriendRequestsSO.Set(view.friendRequestsList.receivedRequestsList.Count());
    }

    private void Entry_OnWhisper(FriendEntry entry)
    {
        OnPressWhisper?.Invoke(entry.model.userName);
    }

    private void Entry_OnReport(FriendEntryBase entry)
    {
        WebInterface.SendReportPlayer(entry.userId);
    }

    private void Entry_OnPassport(FriendEntryBase entry)
    {
        var currentPlayerId = Resources.Load<StringVariable>(CURRENT_PLAYER_ID);
        currentPlayerId.Set(entry.userId);
    }

    private void Entry_OnBlock(FriendEntryBase entry)
    {
        WebInterface.SendBlockPlayer(entry.userId);
    }

    private void Entry_OnJumpIn(FriendEntry entry)
    {
        WebInterface.GoTo((int)entry.model.coords.x, (int)entry.model.coords.y);
    }

    private void Entry_OnDelete(FriendEntryBase entry)
    {
        WebInterface.UpdateFriendshipStatus(
            new FriendsController.FriendshipUpdateStatusMessage()
            {
                action = FriendsController.FriendshipAction.DELETED,
                userId = entry.userId
            });
    }

    private void Entry_OnRequestRejected(FriendRequestEntry entry)
    {
        WebInterface.UpdateFriendshipStatus(
            new FriendsController.FriendshipUpdateStatusMessage()
            {
                action = FriendsController.FriendshipAction.REJECTED,
                userId = entry.userId
            });
    }

    private void Entry_OnRequestCancelled(FriendRequestEntry entry)
    {
        WebInterface.UpdateFriendshipStatus(
            new FriendsController.FriendshipUpdateStatusMessage()
            {
                action = FriendsController.FriendshipAction.CANCELLED,
                userId = entry.userId
            });
    }

    private void Entry_OnRequestAccepted(FriendRequestEntry entry)
    {
        WebInterface.UpdateFriendshipStatus(
            new FriendsController.FriendshipUpdateStatusMessage()
            {
                action = FriendsController.FriendshipAction.APPROVED,
                userId = entry.userId
            });
    }

    public void Dispose()
    {
        if (this.friendsController != null)
        {
            this.friendsController.OnUpdateFriendship -= OnUpdateFriendship;
            this.friendsController.OnUpdateUserStatus -= OnUpdateUserStatus;
        }

        toggleTrigger.OnTriggered -= OnHotkeyPress;

        if (view != null)
        {
            UnityEngine.Object.Destroy(view.gameObject);
        }

        if (this.ownUserProfile != null)
            ownUserProfile.OnUpdate -= OnUserProfileUpdate;
    }

    public void SetVisibility(bool visible)
    {
        view.gameObject.SetActive(visible);
    }

}
