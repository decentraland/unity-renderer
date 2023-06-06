namespace DCL.Wallet
{
    public class WalletPlugin : IPlugin
    {
        private readonly WalletSectionHUDController walletSectionController;

        public WalletPlugin()
        {
            walletSectionController = new WalletSectionHUDController(
                WalletSectionHUDComponentView.Create(),
                DataStore.i);
        }

        public void Dispose() =>
            walletSectionController.Dispose();
    }
}
