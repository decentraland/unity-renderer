namespace DCL.MyAccount
{
    public class EmailNotificationsController
    {
        private readonly IEmailNotificationsComponentView view;
        private readonly MyAccountSectionHUDController myAccountSectionHUDController;

        public EmailNotificationsController(
            IEmailNotificationsComponentView view,
            MyAccountSectionHUDController myAccountSectionHUDController)
        {
            this.view = view;
            this.myAccountSectionHUDController = myAccountSectionHUDController;

            view.OnEmailEdited += UpdateEmail;
            view.OnSubscribeToNewsletterEdited += UpdateNewsletterSubscription;
        }

        public void Dispose()
        {
            view.OnEmailEdited -= UpdateEmail;
            view.OnSubscribeToNewsletterEdited -= UpdateNewsletterSubscription;
        }

        private void UpdateEmail(string newEmail)
        {
            myAccountSectionHUDController.ShowAccountSettingsUpdatedToast();
        }

        private void UpdateNewsletterSubscription(bool isSubscribed)
        {
            myAccountSectionHUDController.ShowAccountSettingsUpdatedToast();
        }
    }
}
