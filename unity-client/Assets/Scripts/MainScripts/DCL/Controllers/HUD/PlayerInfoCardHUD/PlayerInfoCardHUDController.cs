using DCL.Interface;
using UnityEngine;

public class PlayerInfoCardHUDController : IHUD
{
    internal const string CURRENT_PLAYER_ID = "CurrentPlayerInfoCardId";

    internal PlayerInfoCardHUDView view;
    internal StringVariable currentPlayerId;
    internal UserProfile currentUserProfile;
    private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();


    public PlayerInfoCardHUDController()
    {
        view = PlayerInfoCardHUDView.CreateView();
        view.Initialize(() => { currentPlayerId.Set(null); }, ReportPlayer, BlockPlayer, UnblockPlayer, AddPlayerAsFriend, CancelInvitation);
        currentPlayerId = Resources.Load<StringVariable>(CURRENT_PLAYER_ID);
        currentPlayerId.OnChange += OnCurrentPlayerIdChanged;
        OnCurrentPlayerIdChanged(currentPlayerId, null);
    }

    private void AddPlayerAsFriend()
    {
        // Add fake action to avoid waiting for kernel
        UserProfileController.i.AddUserProfileToCatalog(new UserProfileModel()
        {
            userId = currentPlayerId,
            name = currentPlayerId
        });
        FriendsController.i.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage()
        {
            userId = currentPlayerId,
            action = FriendsController.FriendshipAction.REQUESTED_TO
        });

        WebInterface.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage() { userId = currentPlayerId, action = FriendsController.FriendshipAction.REQUESTED_TO });
    }
    private void CancelInvitation()
    {
        // Add fake action to avoid waiting for kernel
        FriendsController.i.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage()
        {
            userId = currentPlayerId,
            action = FriendsController.FriendshipAction.CANCELLED
        });

        WebInterface.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage() { userId = currentPlayerId, action = FriendsController.FriendshipAction.CANCELLED });
    }

    internal void OnCurrentPlayerIdChanged(string current, string previous)
    {
        if (currentUserProfile != null)
            currentUserProfile.OnUpdate -= SetUserProfile;

        currentUserProfile = string.IsNullOrEmpty(currentPlayerId) ? null : UserProfileController.userProfilesCatalog.Get(currentPlayerId);

        if (currentUserProfile == null)
        {
            view.SetCardActive(false);
        }
        else
        {
            currentUserProfile.OnUpdate += SetUserProfile;
            SetUserProfile(currentUserProfile);
            view.SetCardActive(true);
        }
    }

    private void SetUserProfile(UserProfile userProfile)
    {
        view.SetUserProfile(userProfile);
    }

    public void SetVisibility(bool visible)
    {
        view.SetVisibility(visible);
    }

    private void BlockPlayer()
    {
        if (ownUserProfile.blocked.Contains(currentUserProfile.userId)) return;
        ownUserProfile.blocked.Add(currentUserProfile.userId);
        view.SetIsBlocked(true);
        WebInterface.SendBlockPlayer(currentUserProfile.userId);
    }

    private void UnblockPlayer()
    {
        if (!ownUserProfile.blocked.Contains(currentUserProfile.userId)) return;
        ownUserProfile.blocked.Remove(currentUserProfile.userId);
        view.SetIsBlocked(false);
        WebInterface.SendUnlockPlayer(currentUserProfile.userId);
    }

    private void ReportPlayer()
    {
        WebInterface.SendReportPlayer(currentPlayerId);
    }

    public void Dispose()
    {
        if (currentUserProfile != null)
            currentUserProfile.OnUpdate -= SetUserProfile;

        if (currentPlayerId != null)
            currentPlayerId.OnChange -= OnCurrentPlayerIdChanged;
    }
}
