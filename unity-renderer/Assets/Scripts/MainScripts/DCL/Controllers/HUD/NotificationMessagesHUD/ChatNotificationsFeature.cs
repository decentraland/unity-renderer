using DCL.Chat.Channels;

namespace DCL.Chat.Notifications
{
    /// <summary>
    /// Plugin feature that initialize the chat notifications feature.
    /// </summary>
    public class ChatNotificationsFeature : IPlugin
    {
        private readonly ChatNotificationController chatNotificationController;

        public ChatNotificationsFeature()
        {
            chatNotificationController = CreateController();
        }

        private ChatNotificationController CreateController() => new ChatNotificationController(DataStore.i,
            MainChatNotificationsComponentView.Create(), TopNotificationComponentView.Create(),
            ChatController.i,
            new UserProfileWebInterfaceBridge(),
            ProfanityFilterSharedInstances.regexFilter);

        public void Dispose()
        {
            chatNotificationController.Dispose();
        }
    }
}