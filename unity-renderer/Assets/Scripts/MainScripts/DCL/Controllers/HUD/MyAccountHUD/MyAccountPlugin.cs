using DCL.Providers;
using DCL.Tasks;
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
                                                     .Instantiate<MyAccountSectionHUDComponentView>("MyAccountHUD", cancellationToken: ct);

            var dataStore = DataStore.i;

            myProfileController = new MyProfileController(myAccountSectionView.CurrentMyProfileView);
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
