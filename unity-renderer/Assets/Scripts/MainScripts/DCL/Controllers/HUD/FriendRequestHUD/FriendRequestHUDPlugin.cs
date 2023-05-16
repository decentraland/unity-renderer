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
            var friendsController = Environment.i.serviceLocator.Get<IFriendsController>();
            DataStore dataStore = DataStore.i;
            var socialAnalytics = new SocialAnalytics(Environment.i.platform.serviceProviders.analytics, userProfileBridge);

            var receivedFriendRequestHUDComponentView = ReceivedFriendRequestHUDComponentView.Create();

            receivedFriendRequestHUDController = new ReceivedFriendRequestHUDController(
                dataStore,
                receivedFriendRequestHUDComponentView,
                new FriendRequestHUDController(receivedFriendRequestHUDComponentView),
                friendsController,
                userProfileBridge,
                socialAnalytics);

            var sendFriendRequestHUDComponentView = SendFriendRequestHUDComponentView.Create();

            sendFriendRequestHUDController = new SendFriendRequestHUDController(sendFriendRequestHUDComponentView,
                new FriendRequestHUDController(sendFriendRequestHUDComponentView),
                dataStore, userProfileBridge, friendsController,
                socialAnalytics);

            sentFriendRequestHUDController = new SentFriendRequestHUDController(SentFriendRequestHUDComponentView.Create(),
                dataStore, userProfileBridge, friendsController,
                socialAnalytics);
        }

        public void Dispose()
        {
            receivedFriendRequestHUDController.Dispose();
            sendFriendRequestHUDController.Dispose();
            sentFriendRequestHUDController.Dispose();
        }
    }
}
