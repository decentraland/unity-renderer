using DCL.Browser;

namespace DCL.Guests.HUD.ConnectWallet
{
    /// <summary>
    /// Plugin feature that initialize the Connect Wallet Modal feature.
    /// </summary>
    public class ConnectWalletModalPlugin : IPlugin
    {
        private readonly ConnectWalletComponentController joinChannelComponentController;

        public ConnectWalletModalPlugin()
        {
            joinChannelComponentController = new ConnectWalletComponentController(
                ConnectWalletComponentView.Create(),
                new WebInterfaceBrowserBridge(),
                new UserProfileWebInterfaceBridge(),
                DataStore.i);
        }

        public void Dispose() { joinChannelComponentController.Dispose(); }
    }
}