using UnityEngine;

namespace DCL.Social.Friends
{
    public class ReceivedFriendRequestHUDPlugin : IPlugin
    {
        private readonly ReceivedFriendRequestHUDController controller;

        public ReceivedFriendRequestHUDPlugin()
        {
            // TODO: inject a valid view
            controller = new ReceivedFriendRequestHUDController(DataStore.i, null, FriendsController.i,
                new UserProfileWebInterfaceBridge(),
                Resources.Load<StringVariable>("CurrentPlayerInfoCardId"));
        }

        public void Dispose() =>
            controller.Dispose();
    }
}
