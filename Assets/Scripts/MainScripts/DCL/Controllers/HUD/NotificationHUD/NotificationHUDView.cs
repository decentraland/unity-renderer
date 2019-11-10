using System;
using UnityEngine;

public class NotificationHUDView : MonoBehaviour
{
    public NotificationFactory notificationFactory;

    [SerializeField]
    private RectTransform notificationPanel;

    public event System.Action<NotificationModel> OnNotificationDismissed;

    private Canvas notificationCanvas;

    private const string VIEW_PATH = "NotificationHUD";
    private const string VIEW_OBJECT_NAME = "_NotificationHUD";

    internal static NotificationHUDView Create()
    {
        NotificationHUDView view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<NotificationHUDView>();
        view.Initialize();
        return view;
    }

    private void Initialize()
    {
        gameObject.name = VIEW_OBJECT_NAME;
    }

    public void ShowNotification(NotificationModel notificationModel)
    {
        if (notificationModel == null) return;

        Notification notification = notificationFactory.CreateNotificationFromType(notificationModel.type, notificationPanel);
        notification.OnNotificationDismissed += DismissNotification;
        notification.Initialize(notificationModel);
    }

    private void DismissNotification(Notification n)
    {
        OnNotificationDismissed?.Invoke(n.notificationModel);
        Destroy(n.gameObject);
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}