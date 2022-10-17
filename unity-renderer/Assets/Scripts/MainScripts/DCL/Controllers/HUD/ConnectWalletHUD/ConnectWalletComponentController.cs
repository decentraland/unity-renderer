using DCL;
using DCL.Interface;
using System;

public class ConnectWalletComponentController : IDisposable
{
    private const string HELP_URL = "https://docs.decentraland.org/player/blockchain-integration/get-a-wallet/";

    private readonly IConnectWalletComponentView connectWalletView;
    private readonly DataStore dataStore;

    private BaseVariable<bool> connectWalletModalVisible => dataStore.HUDs.connectWalletModalVisible;

    public ConnectWalletComponentController(IConnectWalletComponentView connectWalletView, DataStore dataStore)
    {
        this.connectWalletView = connectWalletView;
        this.dataStore = dataStore;

        this.connectWalletView.OnCancel += OnCancelWalletConnection;
        this.connectWalletView.OnConnect += OnConfirmWalletConnection;
        this.connectWalletView.OnHelp += OnConfirmHelp;
        connectWalletModalVisible.OnChange += OnChangeVisibility;
    }

    public void Dispose()
    {
        connectWalletView.OnCancel -= OnCancelWalletConnection;
        connectWalletView.OnConnect -= OnConfirmWalletConnection;
        connectWalletView.OnHelp -= OnConfirmHelp;
        connectWalletModalVisible.OnChange -= OnChangeVisibility;
    }

    private void OnCancelWalletConnection()
    {
        connectWalletView.Hide();
        connectWalletModalVisible.Set(newValue: false, notifyEvent: false);
    }

    private void OnConfirmWalletConnection()
    {
        connectWalletView.Hide();
        connectWalletModalVisible.Set(newValue: false, notifyEvent: false);
        WebInterface.RedirectToSignUp();
    }

    private void OnConfirmHelp() => WebInterface.OpenURL(HELP_URL);

    private void OnChangeVisibility(bool isVisible, bool previousIsVisible)
    {
        if (isVisible)
            connectWalletView.Show();
        else
            connectWalletView.Hide();
    }
}
