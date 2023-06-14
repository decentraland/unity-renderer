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

            var dataStore = DataStore.i;

            myProfileController = new MyProfileController(
                myAccountSectionView.CurrentMyProfileView,
                dataStore,
                new UserProfileWebInterfaceBridge(),
                Environment.i.serviceLocator.Get<INamesService>(),
                new WebInterfaceBrowserBridge());

            myAccountSectionHUDController = new MyAccountSectionHUDController(
                myAccountSectionView,
                dataStore);
        }

        public void Dispose()
        {
            cts.SafeCancelAndDispose();
            myProfileController.Dispose();
            myAccountSectionHUDController.Dispose();
        }
    }
}
