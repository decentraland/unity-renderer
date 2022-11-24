namespace DCL.Social.Friends
{
    public class SendFriendRequestHUDPlugin : IPlugin
    {
        private readonly SendFriendRequestHUDController controller;
        
        public SendFriendRequestHUDPlugin()
        {
            controller = new SendFriendRequestHUDController(SendFriendRequestHUDComponentView.Create(),
                DataStore.i, new UserProfileWebInterfaceBridge(), FriendsController.i);
        }
        
        public void Dispose()
        {
            controller.Dispose();
        }
    }
}