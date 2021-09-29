using DCL.NotificationModel;

public interface INotificationHUDController
{
    void ShowNotification(INotification notification);
    void ShowNotification(Model model);
    void ShowWelcomeNotification();
    void DismissAllNotifications(string groupID);
    void SetActive(bool active);
    void Dispose();
}