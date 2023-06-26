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
                                                                                           .Instantiate<ProfileAdditionalInfoValueListScriptableObject>("Profiles/AdditionalInfo/Countries", "ProfileCountries", ct);

            ProfileAdditionalInfoValueListScriptableObject genderListProvider = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                                                                          .Instantiate<ProfileAdditionalInfoValueListScriptableObject>("Profiles/AdditionalInfo/Genders", "ProfileGenders", ct);

            ProfileAdditionalInfoValueListScriptableObject sexualOrientationProvider = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                                                                                 .Instantiate<ProfileAdditionalInfoValueListScriptableObject>("Profiles/AdditionalInfo/SexualOrientations", "ProfileSexualOrientations", ct);

            ProfileAdditionalInfoValueListScriptableObject employmentStatusProvider = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                                                                                .Instantiate<ProfileAdditionalInfoValueListScriptableObject>("Profiles/AdditionalInfo/EmploymentStatus", "ProfileEmploymentStatus", ct);

            ProfileAdditionalInfoValueListScriptableObject relationshipStatusProvider = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                                                                                  .Instantiate<ProfileAdditionalInfoValueListScriptableObject>("Profiles/AdditionalInfo/RelationshipStatus", "ProfileRelationshipStatus", ct);

            ProfileAdditionalInfoValueListScriptableObject languageListProvider = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                                                                            .Instantiate<ProfileAdditionalInfoValueListScriptableObject>("Profiles/AdditionalInfo/Languages", "ProfileLanguages", ct);

            ProfileAdditionalInfoValueListScriptableObject pronounListProvider = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                                                                           .Instantiate<ProfileAdditionalInfoValueListScriptableObject>("Profiles/AdditionalInfo/Pronouns", "ProfilePronouns", ct);

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
