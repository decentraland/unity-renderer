using DCLServices.WearablesCatalogService;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;

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
            // since this plugin is bound to backpack_editor_v1 we assume that the feature flags has been already initialized
            hudController.Initialize(false, new PreviewCameraRotationController());
        }

        public void Dispose()
        {
            hudController.Dispose();
        }
    }
}
