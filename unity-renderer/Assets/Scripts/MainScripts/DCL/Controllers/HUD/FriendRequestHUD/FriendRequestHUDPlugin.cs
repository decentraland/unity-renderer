using SocialFeaturesAnalytics;
using UnityEngine;

namespace DCL.Social.Friends
{
    public class FriendRequestHUDPlugin : IPlugin
    {
        private readonly ReceivedFriendRequestHUDController receivedFriendRequestHUDController;
        private readonly SendFriendRequestHUDController sendFriendRequestHUDController;
        private readonly SentFriendRequestHUDController sentFriendRequestHUDController;

        public FriendRequestHUDPlugin()
        {
            var userProfileBridge = new UserProfileWebInterfaceBridge();
            FriendsController friendsController = FriendsController.i;
            DataStore dataStore = DataStore.i;
            var socialAnalytics = new SocialAnalytics(Environment.i.platform.serviceProviders.analytics, userProfileBridge);
            StringVariable openPassportVariable = Resources.Load<StringVariable>("CurrentPlayerInfoCardId");

            receivedFriendRequestHUDController = new ReceivedFriendRequestHUDController(
                dataStore,
                ReceivedFriendRequestHUDComponentView.Create(),
                friendsController,
                userProfileBridge,
                openPassportVariable);

            sendFriendRequestHUDController = new SendFriendRequestHUDController(SendFriendRequestHUDComponentView.Create(),
                dataStore, userProfileBridge, friendsController,
                socialAnalytics);

            sentFriendRequestHUDController = new SentFriendRequestHUDController(SentFriendRequestHUDComponentView.Create(),
                dataStore, userProfileBridge, friendsController,
                socialAnalytics,
                openPassportVariable);
        }

        public void Dispose()
        {
            receivedFriendRequestHUDController.Dispose();
            sendFriendRequestHUDController.Dispose();
            sentFriendRequestHUDController.Dispose();
        }
    }
}
