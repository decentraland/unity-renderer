using UnityEngine;

namespace DCL.Social.Friends
{
    public class ReceivedFriendRequestHUDPlugin : IPlugin
    {
        private readonly ReceivedFriendRequestHUDController controller;

        public ReceivedFriendRequestHUDPlugin()
        {
            controller = new ReceivedFriendRequestHUDController(
                DataStore.i,
                ReceivedFriendRequestHUDComponentView.Create(),
                FriendsController.i,
                new UserProfileWebInterfaceBridge(),
                Resources.Load<StringVariable>("CurrentPlayerInfoCardId"));
        }

        public void Dispose() =>
            controller.Dispose();
    }
}
