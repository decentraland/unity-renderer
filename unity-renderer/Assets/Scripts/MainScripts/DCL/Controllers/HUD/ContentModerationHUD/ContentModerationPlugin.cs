using DCL.Browser;
using DCL.Providers;
using DCL.Tasks;
using DCLServices.PlacesAPIService;
using System.Threading;

namespace DCL.ContentModeration
{
    public class ContentModerationPlugin : IPlugin
    {
        private readonly CancellationTokenSource cts = new ();

        private ContentModerationHUDController contentModerationHUDController;

        public ContentModerationPlugin()
        {
            Initialize(cts.Token);
        }

        private async void Initialize(CancellationToken ct)
        {
            var adultContentSceneWarningComponentView = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                                                         .Instantiate<AdultContentSceneWarningComponentView>("AdultContentSceneWarningHUD", "AdultContentSceneWarningHUD", cancellationToken: ct);

            var adultContentAgeConfirmationComponentView = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                                                             .Instantiate<AdultContentAgeConfirmationComponentView>("AdultContentAgeConfirmationHUD", "AdultContentAgeConfirmationHUD", cancellationToken: ct);

            var adultContentEnabledNotificationComponentView = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                                                                 .Instantiate<AdultContentEnabledNotificationComponentView>("AdultContentEnabledNotificationHUD", "AdultContentEnabledNotificationHUD", cancellationToken: ct);

            var contentModerationReportingComponentView = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                                                 .Instantiate<ContentModerationReportingComponentView>("ContentModerationReportingHUD", "ContentModerationReportingHUD", cancellationToken: ct);

            var contentModerationReportingButtonForWorldsComponentView = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                                                                          .Instantiate<ContentModerationReportingButtonComponentView>("ContentModerationButtonForWorldsHUD", "ContentModerationButtonForWorldsHUD", cancellationToken: ct);

            contentModerationHUDController = new ContentModerationHUDController(
                adultContentSceneWarningComponentView,
                adultContentAgeConfirmationComponentView,
                adultContentEnabledNotificationComponentView,
                contentModerationReportingComponentView,
                contentModerationReportingButtonForWorldsComponentView,
                Environment.i.world.state,
                DataStore.i.common,
                DataStore.i.settings,
                DataStore.i.contentModeration,
                new WebInterfaceBrowserBridge(),
                Environment.i.serviceLocator.Get<IPlacesAPIService>(),
                new UserProfileWebInterfaceBridge(),
                new ContentModerationAnalytics(),
                NotificationsController.i);
        }

        public void Dispose()
        {
            cts.SafeCancelAndDispose();
            contentModerationHUDController.Dispose();
        }
    }
}
