using Cysharp.Threading.Tasks;
using DCL;
using DCL.Providers;
using MainScripts.DCL.Controllers.HUD.ToSPopupHUD;

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
            var assetsProvider = Environment.i.platform.serviceLocator.Get<IAddressableResourceProvider>();
            var hudsDataStore = DataStore.i.HUDs;

            var view = await assetsProvider.Instantiate<IToSPopupView>("ToSPopupHUD", "_ToSPopupHUD");
            controller = new ToSPopupController(view, hudsDataStore.tosPopupVisible, new ToSPopupHandler(hudsDataStore.tosPopupVisible));
        }

        public void Dispose()
        {
            controller?.Dispose();
        }
    }
}
