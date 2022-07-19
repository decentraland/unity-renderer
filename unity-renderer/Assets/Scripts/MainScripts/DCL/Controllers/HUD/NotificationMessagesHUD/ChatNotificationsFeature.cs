using DCL;
using DCL.Helpers;

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

    internal virtual ChatNotificationController CreateController() => new ChatNotificationController(DataStore.i, MainChatNotificationsComponentView.Create(), ChatController.i, new UserProfileWebInterfaceBridge());

    public void Dispose()
    {
        chatNotificationController.Dispose();
    }
}