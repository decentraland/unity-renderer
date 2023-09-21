namespace DCL.ContentModeration
{
    public class ContentModerationHUDController
    {
        private readonly IAdultContentSceneWarningComponentView adultContentSceneWarningComponentView;
        private readonly IAdultContentAgeConfirmationComponentView adultContentAgeConfirmationComponentView;
        private readonly IAdultContentEnabledNotificationComponentView adultContentEnabledNotificationComponentView;

        public ContentModerationHUDController(
            IAdultContentSceneWarningComponentView adultContentSceneWarningComponentView,
            IAdultContentAgeConfirmationComponentView adultContentAgeConfirmationComponentView,
            IAdultContentEnabledNotificationComponentView adultContentEnabledNotificationComponentView)
        {
            this.adultContentSceneWarningComponentView = adultContentSceneWarningComponentView;
            this.adultContentAgeConfirmationComponentView = adultContentAgeConfirmationComponentView;
            this.adultContentEnabledNotificationComponentView = adultContentEnabledNotificationComponentView;
        }

        public void Dispose()
        {

        }
    }
}
