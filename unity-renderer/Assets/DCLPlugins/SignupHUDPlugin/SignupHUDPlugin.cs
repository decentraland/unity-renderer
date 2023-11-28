using Cysharp.Threading.Tasks;
using DCL;
using DCL.Browser;
using DCL.Providers;
using DCLServices.SubscriptionsAPIService;
using SignupHUD;

namespace DCLPlugins.SignupHUDPlugin
{
    public class SignupHUDPlugin : IPlugin
    {
        private const string NEW_TOS_AND_EMAIL_SUBSCRIPTION_FF = "new_terms_of_service_and_email_subscription";
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
                var featureFlagDataStore = DataStore.i.Get<DataStore_FeatureFlag>();
                var backpackDataStore = DataStore.i.Get<DataStore_BackpackV2>();
                var commonDataStore = DataStore.i.Get<DataStore_Common>();
                var browserBridge = new WebInterfaceBrowserBridge();
                var subscriptionsAPIService = Environment.i.serviceLocator.Get<ISubscriptionsAPIService>();
                var userProfileWebInterfaceBridge = new UserProfileWebInterfaceBridge();

                bool isNewTermsOfServiceAndEmailSubscriptionEnabled = current.IsFeatureEnabled(NEW_TOS_AND_EMAIL_SUBSCRIPTION_FF);
                var view = await assetsProvider.Instantiate<ISignupHUDView>(
                    isNewTermsOfServiceAndEmailSubscriptionEnabled ? SIGNUP_HUD_V2 : SIGNUP_HUD,
                    $"_{(isNewTermsOfServiceAndEmailSubscriptionEnabled ? SIGNUP_HUD_V2 : SIGNUP_HUD)}");

                controller = new SignupHUDController(
                    analytics,
                    view,
                    loadingScreenDataStore,
                    hudsDataStore,
                    featureFlagDataStore,
                    backpackDataStore,
                    commonDataStore,
                    browserBridge,
                    subscriptionsAPIService);

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
