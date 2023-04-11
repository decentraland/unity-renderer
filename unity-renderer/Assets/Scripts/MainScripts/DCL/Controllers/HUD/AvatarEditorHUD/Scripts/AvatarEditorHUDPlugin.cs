using DCLServices.WearablesCatalogService;

namespace DCL.AvatarEditor
{
    public class AvatarEditorHUDPlugin : IPlugin
    {
        private readonly AvatarEditorHUDController hudController;
        private readonly DataStore_FeatureFlag featureFlags;

        public AvatarEditorHUDPlugin()
        {
            featureFlags = DataStore.i.featureFlags;

            hudController = new AvatarEditorHUDController(
                featureFlags,
                Environment.i.platform.serviceProviders.analytics,
                Environment.i.serviceLocator.Get<IWearablesCatalogService>(),
                new UserProfileWebInterfaceBridge());

            // there is a race condition going on if we initialize the avatar editor before the feature flags are set
            // WearablesCatalogServiceProxy.wearablesCatalogServiceInUse is not defined which is needed to retrieve the wearable catalog
            featureFlags.flags.OnChange += InitializeHUD;
        }

        public void Dispose()
        {
            featureFlags.flags.OnChange -= InitializeHUD;
            hudController.Dispose();
        }

        private void InitializeHUD(FeatureFlag current, FeatureFlag previous)
        {
            featureFlags.flags.OnChange -= InitializeHUD;

            hudController.Initialize();
        }
    }
}
