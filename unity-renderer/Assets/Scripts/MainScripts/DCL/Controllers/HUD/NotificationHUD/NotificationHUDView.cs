using System;
using DCL.NotificationModel;
using UnityEngine;

public class NotificationHUDView : MonoBehaviour
{
    public NotificationFactory notificationFactory;

    [SerializeField]
    private RectTransform notificationPanel;

    public event Action<INotification> OnNotificationDismissedEvent;

    private const string VIEW_PATH = "NotificationHUD";
    private const string VIEW_OBJECT_NAME = "_NotificationHUD";

    internal static NotificationHUDView Create()
    {
        NotificationHUDView view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<NotificationHUDView>();
        view.Initialize();
        return view;
    }

    private void Initialize() { gameObject.name = VIEW_OBJECT_NAME; }

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
}