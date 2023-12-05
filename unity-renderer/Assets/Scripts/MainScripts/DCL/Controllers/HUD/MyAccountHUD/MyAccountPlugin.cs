using DCL.Browser;
using DCL.Providers;
using DCL.Tasks;
using DCLServices.Lambdas.NamesService;
using DCLServices.SubscriptionsAPIService;
using SocialFeaturesAnalytics;
using System.Threading;

namespace DCL.MyAccount
{
    public class MyAccountPlugin : IPlugin
    {
        private readonly CancellationTokenSource cts = new ();

        private MyAccountSectionHUDController myAccountSectionHUDController;
        private MyProfileController myProfileController;
        private EmailNotificationsController emailNotificationsController;

        public MyAccountPlugin()
        {
            Initialize(cts.Token);
        }

        private async void Initialize(CancellationToken ct)
        {
            var myAccountSectionView = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                                        .Instantiate<MyAccountSectionHUDComponentView>("MyAccountSectionHUD", "MyAccountSectionHUD", cancellationToken: ct);

            var updateEmailConfirmationHUD = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                                              .Instantiate<UpdateEmailConfirmationHUDComponentView>("UpdateEmailConfirmationHUD", "UpdateEmailConfirmationHUD", cancellationToken: ct);

            ProfileAdditionalInfoValueListScriptableObject countryListProvider = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                                                                                  .GetAddressable<ProfileAdditionalInfoValueListScriptableObject>("ProfileCountries", ct);

            ProfileAdditionalInfoValueListScriptableObject genderListProvider = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                                                                          .GetAddressable<ProfileAdditionalInfoValueListScriptableObject>("ProfileGenders", ct);

            ProfileAdditionalInfoValueListScriptableObject sexualOrientationProvider = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                                                                                 .GetAddressable<ProfileAdditionalInfoValueListScriptableObject>("ProfileSexualOrientations", ct);

            ProfileAdditionalInfoValueListScriptableObject employmentStatusProvider = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                                                                                .GetAddressable<ProfileAdditionalInfoValueListScriptableObject>("ProfileEmploymentStatus", ct);

            ProfileAdditionalInfoValueListScriptableObject relationshipStatusProvider = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                                                                                  .GetAddressable<ProfileAdditionalInfoValueListScriptableObject>("ProfileRelationshipStatus", ct);

            ProfileAdditionalInfoValueListScriptableObject languageListProvider = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                                                                            .GetAddressable<ProfileAdditionalInfoValueListScriptableObject>("ProfileLanguages", ct);

            ProfileAdditionalInfoValueListScriptableObject pronounListProvider = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                                                                           .GetAddressable<ProfileAdditionalInfoValueListScriptableObject>("ProfilePronouns", ct);

            var dataStore = DataStore.i;

            var userProfileWebInterfaceBridge = new UserProfileWebInterfaceBridge();

            var socialAnalytics = new SocialAnalytics(Environment.i.platform.serviceProviders.analytics, userProfileWebInterfaceBridge);

            myAccountSectionHUDController = new MyAccountSectionHUDController(
                myAccountSectionView,
                dataStore);

            myProfileController = new MyProfileController(
                myAccountSectionView.CurrentMyProfileView,
                dataStore,
                userProfileWebInterfaceBridge,
                Environment.i.serviceLocator.Get<INamesService>(),
                new WebInterfaceBrowserBridge(),
                myAccountSectionHUDController,
                KernelConfig.i,
                new MyAccountAnalyticsService(Environment.i.platform.serviceProviders.analytics),
                socialAnalytics,
                countryListProvider,
                genderListProvider,
                sexualOrientationProvider,
                employmentStatusProvider,
                relationshipStatusProvider,
                languageListProvider,
                pronounListProvider);

            emailNotificationsController = new EmailNotificationsController(
                myAccountSectionView.CurrentEmailNotificationsView,
                updateEmailConfirmationHUD,
                myAccountSectionHUDController,
                dataStore,
                Environment.i.serviceLocator.Get<ISubscriptionsAPIService>(),
                socialAnalytics);
        }

        public void Dispose()
        {
            cts.SafeCancelAndDispose();
            myProfileController.Dispose();
            emailNotificationsController.Dispose();
            myAccountSectionHUDController.Dispose();
        }
    }
}
