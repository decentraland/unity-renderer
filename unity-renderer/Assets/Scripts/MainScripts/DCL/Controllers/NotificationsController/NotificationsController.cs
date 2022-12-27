using DCL;
using DCL.NotificationModel;
using UnityEngine;

public class NotificationsController : MonoBehaviour
{
    public static NotificationsController i { get; private set; }
    public static bool disableWelcomeNotification = false;

    public bool allowNotifications { get; set; }

    void Awake() { i = this; }

    private INotificationHUDController controller;
    private DataStore_Notifications notificationsDataStore;

    // TODO: refactor into a bridge->service architecture so this dependencies are properly injected
    public void Initialize(
        INotificationHUDController controller,
        DataStore_Notifications notificationsDataStore)
    {
        this.controller = controller;
        this.notificationsDataStore = notificationsDataStore;
        allowNotifications = true;

        this.notificationsDataStore.DefaultErrorNotification.OnChange += ShowDefaultErrorNotification;
    }

    public void Dispose()
    {
        notificationsDataStore.DefaultErrorNotification.OnChange -= ShowDefaultErrorNotification;
        controller.Dispose();
    }

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

    private void ShowDefaultErrorNotification(string errorText, string _)
    {
        ShowNotification(new Model
        {
            message = errorText,
            type = Type.ERROR,
            timer = 5f,
            destroyOnFinish = true
        });
    }
}
