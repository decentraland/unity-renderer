namespace DCL.ContentModeration
{
    public class AdultContentEnabledNotificationComponentView : BaseComponentView, IAdultContentEnabledNotificationComponentView
    {
        public override void RefreshControl() { }

        public void ShowNotification() =>
            Show();

        public void HideNotification() =>
            Hide();
    }
}
