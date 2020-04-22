public class NotificationHUDController : IHUD
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

    public void SetActive(bool active)
    {
        view.SetActive(active);
    }

    public void Dispose()
    {
        view.OnNotificationDismissed -= DismissNotification;
    }

    public void SetVisibility(bool visible)
    {
        SetActive(visible);
    }
}
