using DCL.NotificationModel;
using UnityEngine;

public class NotificationsController : MonoBehaviour
{
    public static NotificationsController i { get; private set; }
    public static bool disableWelcomeNotification = false;

    public bool allowNotifications { get; set; }

    void Awake() { i = this; }

    INotificationHUDController controller;

    public void Initialize(INotificationHUDController controller)
    {
        this.controller = controller;
        allowNotifications = true;
    }

    public void Dispose() { controller.Dispose(); }

    public void ShowNotificationFromJson(string notificationJson)
    {
        if (!allowNotifications)
            return;

        Model model = JsonUtility.FromJson<Model>(notificationJson);
        ShowNotification(model);
    }

    public void ShowNotification(Model notification)
    {
        if (!allowNotifications)
            return;

        controller?.ShowNotification(notification);
    }

    public void ShowNotification(INotification notification)
    {
        if (!allowNotifications)
            return;

        controller?.ShowNotification(notification);
    }

    public void DismissAllNotifications(string groupID) { controller?.DismissAllNotifications(groupID); }

    public void ShowWelcomeNotification()
    {
        if (!allowNotifications || disableWelcomeNotification)
            return;

        //TODO(Brian): This should be triggered entirely by kernel
        controller?.ShowWelcomeNotification();
    }
}