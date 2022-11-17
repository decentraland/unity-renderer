using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL;
using DCL.Helpers;

public class PassportPlayerInfoComponentController
{
    private IPassportPlayerInfoComponentView view;
    private readonly DataStore dataStore;
    private readonly IProfanityFilter profanityFilter;
    private readonly IFriendsController friendsController;
    private readonly IUserProfileBridge userProfileBridge;

    private StringVariable currentPlayerId;

    public PassportPlayerInfoComponentController(
        StringVariable currentPlayerId,
        IPassportPlayerInfoComponentView view, 
        DataStore dataStore, 
        IProfanityFilter profanityFilter,
        IFriendsController friendsController,
        IUserProfileBridge userProfileBridge)
    {
        this.currentPlayerId = currentPlayerId;
        this.view = view;
        this.dataStore = dataStore;
        this.profanityFilter = profanityFilter;
        this.friendsController = friendsController;
        this.userProfileBridge = userProfileBridge;

        view.OnAddFriend += AddPlayerAsFriend;
    }

    public async UniTask UpdateWithUserProfile(UserProfile userProfile)
    {
        view.SetWallet(userProfile.userId);
        string filteredName = await FilterName(userProfile);
        view.SetName(filteredName);
        view.SetPresence(friendsController.GetUserStatus(userProfile.userId).presence);
    }

    private async UniTask<string> FilterName(UserProfile userProfile)
    {
        return IsProfanityFilteringEnabled()
            ? await profanityFilter.Filter(userProfile.userName)
            : userProfile.userName;
    }

    private bool IsProfanityFilteringEnabled()
    {
        return dataStore.settings.profanityChatFilteringEnabled.Get();
    }

    private void AddPlayerAsFriend()
    {
        UserProfile currentUserProfile = userProfileBridge.Get(currentPlayerId);

        // Add fake action to avoid waiting for kernel
        userProfileBridge.AddUserProfileToCatalog(new UserProfileModel
        {
            userId = currentPlayerId,
            name = currentUserProfile != null ? currentUserProfile.userName : currentPlayerId
        });

        friendsController.RequestFriendship(currentPlayerId);
        //socialAnalytics.SendFriendRequestSent(ownUserProfile.userId, currentPlayerId, 0, PlayerActionSource.Passport);
    }
}
