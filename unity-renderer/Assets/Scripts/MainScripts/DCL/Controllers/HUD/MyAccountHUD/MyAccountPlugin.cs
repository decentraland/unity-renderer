using DCL.Browser;
using DCL.Providers;
using DCL.Tasks;
using DCLServices.Lambdas.NamesService;
using System.Threading;

namespace DCL.MyAccount
{
    public class MyAccountPlugin : IPlugin
    {
        private readonly CancellationTokenSource cts = new ();

        private MyAccountSectionHUDController myAccountSectionHUDController;
        private MyProfileController myProfileController;

        public MyAccountPlugin()
        {
            Initialize(cts.Token);
        }

        private async void Initialize(CancellationToken ct)
        {
            var myAccountSectionView = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                                        .Instantiate<MyAccountSectionHUDComponentView>("MyAccountSectionHUD", "MyAccountSectionHUD", cancellationToken: ct);

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

            myAccountSectionHUDController = new MyAccountSectionHUDController(
                myAccountSectionView,
                dataStore);

            myProfileController = new MyProfileController(
                myAccountSectionView.CurrentMyProfileView,
                dataStore,
                new UserProfileWebInterfaceBridge(),
                Environment.i.serviceLocator.Get<INamesService>(),
                new WebInterfaceBrowserBridge(),
                myAccountSectionHUDController,
                KernelConfig.i,
                new MyAccountAnalyticsService(Environment.i.platform.serviceProviders.analytics),
                countryListProvider,
                genderListProvider,
                sexualOrientationProvider,
                employmentStatusProvider,
                relationshipStatusProvider,
                languageListProvider,
                pronounListProvider);
        }

        public void Dispose()
        {
            cts.SafeCancelAndDispose();
            myProfileController.Dispose();
            myAccountSectionHUDController.Dispose();
        }
    }
}
