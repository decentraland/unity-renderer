using Cysharp.Threading.Tasks;
using DCL;
using DCL.Providers;
using SignupHUD;

namespace DCLPlugins.SignupHUDPlugin
{
    public class SignupHUDPlugin : IPlugin
    {
        private const string SIGNUP_HUD = "SignupHUD";
        private const string SIGNUP_HUD_V2 = "SignupHUDV2";

        private BaseVariable<FeatureFlag> featureFlags => DataStore.i.featureFlags.flags;

        private SignupHUDController controller;

        public SignupHUDPlugin()
        {
            Initialize().Forget();
        }

        private async UniTaskVoid Initialize()
        {
            if (!featureFlags.Get().IsInitialized)
                featureFlags.OnChange += OnFeatureFlagsChanged;
            else
                OnFeatureFlagsChanged(featureFlags.Get());
        }

        private void OnFeatureFlagsChanged(FeatureFlag current, FeatureFlag _ = null)
        {
            async UniTaskVoid InitializeController()
            {
                var assetsProvider = DCL.Environment.i.platform.serviceLocator.Get<IAddressableResourceProvider>();
                var analytics = DCL.Environment.i.platform.serviceProviders.analytics;
                var loadingScreenDataStore = DataStore.i.Get<DataStore_LoadingScreen>();
                var hudsDataStore = DataStore.i.HUDs;

                bool isNewTermsOfServiceAndEmailSubscriptionEnabled = current.IsFeatureEnabled("new_terms_of_service_and_email_subscription");
                var view = await assetsProvider.Instantiate<ISignupHUDView>(
                    isNewTermsOfServiceAndEmailSubscriptionEnabled ? SIGNUP_HUD_V2 : SIGNUP_HUD,
                    $"_{(isNewTermsOfServiceAndEmailSubscriptionEnabled ? SIGNUP_HUD_V2 : SIGNUP_HUD)}");

                controller = new SignupHUDController(analytics, view, loadingScreenDataStore, hudsDataStore);
                controller.Initialize();
            }

            featureFlags.OnChange -= OnFeatureFlagsChanged;
            InitializeController().Forget();
        }

        public void Dispose()
        {
            featureFlags.OnChange -= OnFeatureFlagsChanged;
            controller?.Dispose();
        }
    }
}
