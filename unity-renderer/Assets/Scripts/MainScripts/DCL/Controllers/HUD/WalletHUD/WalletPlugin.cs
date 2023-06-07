using DCL.Browser;

namespace DCL.Wallet
{
    public class WalletPlugin : IPlugin
    {
        private readonly WalletSectionHUDController walletSectionController;

        public WalletPlugin()
        {
            walletSectionController = new WalletSectionHUDController(
                WalletSectionHUDComponentView.Create(),
                DataStore.i,
                new UserProfileWebInterfaceBridge(),
                Environment.i.platform.clipboard,
                new WebInterfaceBrowserBridge(),
                Environment.i.platform.serviceProviders.theGraph);
        }

        public void Dispose() =>
            walletSectionController.Dispose();
    }
}
