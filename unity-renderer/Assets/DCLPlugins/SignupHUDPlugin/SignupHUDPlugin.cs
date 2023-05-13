using Cysharp.Threading.Tasks;
using DCL;
using DCL.Providers;
using SignupHUD;

namespace DCLPlugins.SignupHUDPlugin
{
    public class SignupHUDPlugin : IPlugin
    {
        private const string SIGNUP_HUD = "SignupHUD";

        private SignupHUDController controller;

        public SignupHUDPlugin()
        {
            Initialize().Forget();
        }

        private async UniTaskVoid Initialize()
        {
            var assetsProvider = DCL.Environment.i.platform.serviceLocator.Get<IAddressableResourceProvider>();
            var analytics = DCL.Environment.i.platform.serviceProviders.analytics;
            var loadingScreenDataStore = DataStore.i.Get<DataStore_LoadingScreen>();
            var hudsDataStore = DataStore.i.HUDs;

            var view = await assetsProvider.Instantiate<ISignupHUDView>(SIGNUP_HUD, $"_{SIGNUP_HUD}");
            controller = new SignupHUDController(analytics, view, loadingScreenDataStore, hudsDataStore);
            controller.Initialize();
        }

        public void Dispose()
        {
            controller?.Dispose();
        }
    }
}
