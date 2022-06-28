/// <summary>
/// Plugin feature that initialize the chat notifications feature.
/// </summary>
public class ChatNotificationsFeature : IPlugin
{
    public ChatNotificationController chatController;

    public ChatNotificationsFeature()
    {
        chatController = CreateController();
        //chatController.Initialize(Environment.i.world.sceneController);
    }

    internal virtual ChatNotificationController CreateController() => new ChatNotificationController(ChatController.i);

    public void Dispose()
    {
        //chatController.Dispose();
    }
}