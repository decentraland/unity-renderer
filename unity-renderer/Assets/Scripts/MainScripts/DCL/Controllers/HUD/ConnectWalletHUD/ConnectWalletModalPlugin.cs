using DCL;

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
            DataStore.i);
    }

    public void Dispose() { joinChannelComponentController.Dispose(); }
}