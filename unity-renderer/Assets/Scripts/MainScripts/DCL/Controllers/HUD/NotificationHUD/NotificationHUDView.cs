using System;
using DCL.NotificationModel;
using UnityEngine;

public class NotificationHUDView : MonoBehaviour, IDisposable
{
    public NotificationFactory notificationFactory;

    [SerializeField]
    private RectTransform notificationPanel;

    public event Action<INotification> OnNotificationDismissedEvent;

    public void ShowNotification(INotification notification, Model model = null)
    {
        if (notification == null)
            return;

        notification.OnNotificationDismissed += OnNotificationDismissed;

        if (model != null)
            notification.Show(model);
    }

    public Notification ShowNotification(Model notificationModel)
    {
        if (notificationModel == null)
            return null;

        Notification notification = notificationFactory.CreateNotificationFromType(notificationModel.type, notificationPanel);
        notificationModel.destroyOnFinish = true;
        ShowNotification(notification, notificationModel);
        return notification;
    }

    private void OnNotificationDismissed(INotification notification) { OnNotificationDismissedEvent?.Invoke(notification); }

    public void SetActive(bool active) { gameObject.SetActive(active); }

    public void Dispose()
    {
    }
}
