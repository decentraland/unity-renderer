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
                KernelConfig.i);
        }

        public void Dispose()
        {
            cts.SafeCancelAndDispose();
            myProfileController.Dispose();
            myAccountSectionHUDController.Dispose();
        }
    }
}
