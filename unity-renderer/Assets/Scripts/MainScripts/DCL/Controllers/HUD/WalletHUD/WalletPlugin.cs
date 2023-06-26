using DCL.Browser;
using DCL.Guests.HUD.ConnectWallet;
using DCL.MyAccount;
using DCL.Providers;
using DCL.Tasks;
using System.Threading;

namespace DCL.Wallet
{
    public class WalletPlugin : IPlugin
    {
        private readonly CancellationTokenSource cts = new ();

        private WalletSectionHUDController walletSectionController;
        private ConnectWalletComponentController connectWalletController;

        public WalletPlugin()
        {
            Initialize(cts.Token);
        }

        private async void Initialize(CancellationToken ct)
        {
            var walletSectionView = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                                     .Instantiate<WalletSectionHUDComponentView>("WalletSectionHUD", cancellationToken: ct);
            var dataStore = DataStore.i;
            var userProfileWebInterfaceBridge = new UserProfileWebInterfaceBridge();
            var webInterfaceBrowserBridge = new WebInterfaceBrowserBridge();

            walletSectionController = new WalletSectionHUDController(
                walletSectionView,
                dataStore,
                userProfileWebInterfaceBridge,
                Environment.i.platform.clipboard,
                webInterfaceBrowserBridge,
                Environment.i.platform.serviceProviders.theGraph,
                new MyAccountAnalyticsService(Environment.i.platform.serviceProviders.analytics));

            connectWalletController = new ConnectWalletComponentController(
                walletSectionView.connectWalletView,
                webInterfaceBrowserBridge,
                userProfileWebInterfaceBridge,
                dataStore);
        }

        public void Dispose()
        {
            cts.SafeCancelAndDispose();
            walletSectionController.Dispose();
            connectWalletController.Dispose();
        }
    }
}
