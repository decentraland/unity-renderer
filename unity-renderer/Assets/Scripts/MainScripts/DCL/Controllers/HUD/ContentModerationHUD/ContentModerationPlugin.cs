using DCL.Providers;
using DCL.Tasks;
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

            contentModerationHUDController = new ContentModerationHUDController(
                adultContentSceneWarningComponentView,
                adultContentAgeConfirmationComponentView,
                adultContentEnabledNotificationComponentView,
                Environment.i.world.state,
                DataStore.i.settings,
                DataStore.i.contentModeration);
        }

        public void Dispose()
        {
            cts.SafeCancelAndDispose();
            contentModerationHUDController.Dispose();
        }
    }
}
