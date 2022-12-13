using SocialFeaturesAnalytics;
using UnityEngine;

namespace DCL.Social.Friends
{
    public class CancelFriendRequestHUDPlugin : IPlugin
    {
        private readonly CancelFriendRequestHUDController controller;

        public CancelFriendRequestHUDPlugin()
        {
            var userProfileBridge = new UserProfileWebInterfaceBridge();
            controller = new CancelFriendRequestHUDController(CancelFriendRequestHUDComponentView.Create(),
                DataStore.i, userProfileBridge, FriendsController.i,
                new SocialAnalytics(Environment.i.platform.serviceProviders.analytics, userProfileBridge),
                Resources.Load<StringVariable>("CurrentPlayerInfoCardId"));
        }

        public void Dispose()
        {
            controller.Dispose();
        }
    }
}
