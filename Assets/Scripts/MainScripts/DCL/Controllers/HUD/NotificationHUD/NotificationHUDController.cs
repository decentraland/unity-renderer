using System;

public class NotificationHUDController : IDisposable
{
    public NotificationHUDView view { get; private set; }
    public NotificationHUDModel model { get; private set; }
    public NotificationHUDController() : this(new NotificationHUDModel()) { }

    public NotificationHUDController(NotificationHUDModel model)
    {
        this.model = model;
        view = NotificationHUDView.Create();
        view.OnNotificationDismissed += DismissNotification;
    }

    public void ShowNotification(NotificationModel notification)
    {
        model.notifications.Add(notification);
        view.ShowNotification(notification);
    }

    private void DismissNotification(NotificationModel notification)
    {
        model.notifications.Remove(notification);
    }

    public void Dispose()
    {
        view.OnNotificationDismissed -= DismissNotification;
    }
}