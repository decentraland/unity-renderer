using DCLServices.WearablesCatalogService;

namespace DCL.AvatarEditor
{
    public class AvatarEditorHUDPlugin : IPlugin
    {
        private readonly AvatarEditorHUDController hudController;

        public AvatarEditorHUDPlugin()
        {
            hudController = new AvatarEditorHUDController(
                DataStore.i.featureFlags,
                Environment.i.platform.serviceProviders.analytics,
                Environment.i.serviceLocator.Get<IWearablesCatalogService>(),
                new UserProfileWebInterfaceBridge());

            // there could be a race condition going on if we initialize the avatar editor before the feature flags are set
            // WearablesCatalogServiceProxy.wearablesCatalogServiceInUse is not defined when is needed to retrieve the wearable catalog
            hudController.Initialize();
        }

        public void Dispose()
        {
            hudController.Dispose();
        }
    }
}
