using UnityEngine;

public class NotificationsController : MonoBehaviour
{
    const int NOTIFICATION_DURATION = 5;

    public static NotificationsController i { get; private set; }

    void Awake()
    {
        i = this;
    }

    NotificationHUDController controller;
    public void Initialize(NotificationHUDController controller)
    {
        this.controller = controller;
    }

    public void ShowNotificationFromJson(string notificationJson)
    {
        Notification.Model model = JsonUtility.FromJson<Notification.Model>(notificationJson);
        ShowNotification(model);
    }

    public void ShowNotification(Notification.Model notification)
    {
        controller.ShowNotification(notification);
    }

    public void ShowNotification(Notification notification)
    {
        controller.ShowNotification(notification);
    }

    public void DismissAllNotifications(string groupID)
    {
        controller.DismissAllNotifications(groupID);
    }

    public void ShowWelcomeNotification()
    {
        //TODO(Brian): This should be triggered entirely by kernel
        string notificationText = $"Welcome, {UserProfile.GetOwnUserProfile().userName}!";
        Vector2Int currentCoords = CommonScriptableObjects.playerCoords.Get();
        string parcelName = MinimapMetadata.GetMetadata().GetSceneInfo(currentCoords.x, currentCoords.y)?.name;

        if (!string.IsNullOrEmpty(parcelName))
        {
            notificationText += $" You are in {parcelName} {currentCoords.x}, {currentCoords.y}";
        }

        Notification.Model model = new Notification.Model()
        {
            message = notificationText,
            scene = "",
            type = NotificationFactory.Type.GENERIC_WITHOUT_BUTTON,
            timer = NOTIFICATION_DURATION
        };

        controller.ShowNotification(model);
    }

}
