/// <summary>
/// Plugin feature that initialize the chat notifications feature.
/// </summary>
public class ChatNotificationsFeature : IPlugin
{
    public ChatNotificationController chatNotificationController;

    public ChatNotificationsFeature()
    {
        chatNotificationController = CreateController();
    }

    internal virtual ChatNotificationController CreateController() => new ChatNotificationController(ChatController.i, new UserProfileWebInterfaceBridge(), MainChatNotificationsComponentView.Create());

    public void Dispose()
    {
        chatNotificationController.Dispose();
    }
}