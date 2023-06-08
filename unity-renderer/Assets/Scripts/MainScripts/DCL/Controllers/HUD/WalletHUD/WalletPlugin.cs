using DCL.Browser;
using DCL.Guests.HUD.ConnectWallet;

namespace DCL.Wallet
{
    public class WalletPlugin : IPlugin
    {
        private readonly WalletSectionHUDController walletSectionController;
        private readonly ConnectWalletComponentController connectWalletController;

        public WalletPlugin()
        {
            var walletSectionView = WalletSectionHUDComponentView.Create();
            var dataStore = DataStore.i;
            var userProfileWebInterfaceBridge = new UserProfileWebInterfaceBridge();
            var webInterfaceBrowserBridge = new WebInterfaceBrowserBridge();

            walletSectionController = new WalletSectionHUDController(
                walletSectionView,
                dataStore,
                userProfileWebInterfaceBridge,
                Environment.i.platform.clipboard,
                webInterfaceBrowserBridge,
                Environment.i.platform.serviceProviders.theGraph);

            connectWalletController = new ConnectWalletComponentController(
                walletSectionView.connectWalletView,
                webInterfaceBrowserBridge,
                userProfileWebInterfaceBridge,
                dataStore);
        }

        public void Dispose()
        {
            walletSectionController.Dispose();
            connectWalletController.Dispose();
        }
    }
}
