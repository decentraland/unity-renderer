using Cysharp.Threading.Tasks;
using DCl.Social.Friends;

namespace DCL.Social.Passports
{
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

        public void UpdateWithUserProfile(UserProfile userProfile) => UpdateWithUserProfileAsync(userProfile);

        private async UniTask UpdateWithUserProfileAsync(UserProfile userProfile)
        {
            string filteredName = await FilterName(userProfile);
            view.SetName(filteredName);
            view.SetGuestUser(userProfile.isGuest);
            if(!userProfile.isGuest)
            {
                view.SetWallet(userProfile.userId);
                view.SetPresence(friendsController.GetUserStatus(userProfile.userId).presence);
            }
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

            friendsController.RequestFriendship(currentPlayerId);
            //socialAnalytics.SendFriendRequestSent(ownUserProfile.userId, currentPlayerId, 0, PlayerActionSource.Passport);
        }
    }
}