using System.Collections.Generic;

public class NotificationHUDController : IHUD, INotificationHUDController
{
    [System.Serializable]
    public class Model
    {
        public List<INotification> notifications = new List<INotification>();
    }

    public NotificationHUDView view { get; private set; }
    public Model model { get; private set; }

    public NotificationHUDController() : this(new Model()) { }

    public NotificationHUDController(Model model)
    {
        this.model = model;
        view = NotificationHUDView.Create();
        view.OnNotificationDismissedEvent += OnNotificationDismissed;
    }

    public void ShowNotification(INotification notification)
    {
        model.notifications.Add(notification);
        view.ShowNotification(notification, notification.model);
    }

    public void ShowNotification(DCL.NotificationModel.Model model)
    {
        if (!string.IsNullOrEmpty(model.groupID))
        {
            //NOTE(Brian): If more notifications of the same group are visible, hide them.
            //             This works when having notifications in different contexts (i.e. FriendsHUD vs global notifications)
            DismissAllNotifications(model.groupID);
        }

        var notification = view.ShowNotification(model);
        this.model.notifications.Add(notification);
    }

    public void DismissAllNotifications(string groupID)
    {
        if (this.model.notifications.Count > 0)
        {
            //NOTE(Brian): Copy list to avoid modify while iterating error
            var notiList = new List<INotification>(this.model.notifications);
            int notiCount = notiList.Count;

            for (int i = 0; i < notiCount; i++)
            {
                if (!string.IsNullOrEmpty(groupID) && groupID != notiList[i].model.groupID)
                    continue;

                notiList[i].Dismiss(instant: true);
            }
        }
    }

    private void OnNotificationDismissed(INotification notification) { model.notifications.Remove(notification); }

    public void SetActive(bool active) { view.SetActive(active); }

    public void Dispose()
    {
        if (view != null)
        {
            UnityEngine.Object.Destroy(view.gameObject);
        }

        view.OnNotificationDismissedEvent -= OnNotificationDismissed;
    }

    public void SetVisibility(bool visible) { SetActive(visible); }

    public void ShowWelcomeNotification()
    {
        var model = WelcomeNotification.Get();
        ShowNotification(model);
    }
}