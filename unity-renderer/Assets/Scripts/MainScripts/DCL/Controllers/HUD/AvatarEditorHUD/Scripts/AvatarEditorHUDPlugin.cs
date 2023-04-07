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
            hudController.Initialize();
        }

        public void Dispose()
        {
            hudController.Dispose();
        }
    }
}
