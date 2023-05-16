using Cysharp.Threading.Tasks;
using DCL;
using DCL.Providers;

namespace DCLPlugins.ToSPopupHUDPlugin
{
    public class ToSPopupHUDPlugin : IPlugin
    {
        private ToSPopupController controller;

        public ToSPopupHUDPlugin()
        {
            Initialize().Forget();
        }

        private async UniTaskVoid Initialize()
        {
            await Environment.WaitUntilInitialized();
            IAddressableResourceProvider assetsProvider = Environment.i.platform.serviceLocator.Get<IAddressableResourceProvider>();

            //var analytics = Environment.i.platform.serviceProviders.analytics;
            var hudsDataStore = DataStore.i.HUDs;

            var view = await assetsProvider.Instantiate<IToSPopupView>("ToSPopupHUD", "_ToSPopupHUD");
            controller = new ToSPopupController(view, new ToSPopupHandler());

            //Enable the popup, the plugin depens on the feature flag
            hudsDataStore.tosPopupVisible.Set(true, true);
        }

        public void Dispose()
        {
            controller?.Dispose();
        }
    }
}
