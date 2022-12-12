using SocialFeaturesAnalytics;

namespace DCL.Social.Friends
{
    public class SendFriendRequestHUDPlugin : IPlugin
    {
        private readonly SendFriendRequestHUDController controller;

        public SendFriendRequestHUDPlugin()
        {
            var userProfileBridge = new UserProfileWebInterfaceBridge();
            controller = new SendFriendRequestHUDController(SendFriendRequestHUDComponentView.Create(),
                DataStore.i, userProfileBridge, FriendsController.i,
                new SocialAnalytics(Environment.i.platform.serviceProviders.analytics, userProfileBridge));
        }

        public void Dispose()
        {
            controller.Dispose();
        }
    }
}
