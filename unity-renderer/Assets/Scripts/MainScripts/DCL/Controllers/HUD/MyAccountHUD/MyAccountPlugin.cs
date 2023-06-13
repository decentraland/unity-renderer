using DCL.Providers;
using DCL.Tasks;
using System.Threading;

namespace DCL.MyAccount
{
    public class MyAccountPlugin : IPlugin
    {
        private readonly CancellationTokenSource cts = new ();

        private MyAccountHUDController myAccountHUDController;
        private MyProfileController myProfileController;

        public MyAccountPlugin()
        {
            Initialize(cts.Token);
        }

        private async void Initialize(CancellationToken ct)
        {
            var myAccountSectionView = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                                     .Instantiate<MyAccountHUDComponentView>("MyAccountHUD", cancellationToken: ct);

            var dataStore = DataStore.i;

            myProfileController = new MyProfileController(myAccountSectionView.CurrentMyProfileView);
            myAccountHUDController = new MyAccountHUDController(
                myAccountSectionView,
                dataStore);
        }

        public void Dispose()
        {
            cts.SafeCancelAndDispose();
            myProfileController.Dispose();
            myAccountHUDController.Dispose();
        }
    }
}
