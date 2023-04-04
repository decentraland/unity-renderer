using DCL.Browser;
using System;

namespace DCL.Guests.HUD.ConnectWallet
{
    public class ConnectWalletComponentController : IDisposable
    {
        private const string HELP_URL = "https://docs.decentraland.org/player/blockchain-integration/get-a-wallet/";

        private readonly IConnectWalletComponentView connectWalletView;
        private readonly IBrowserBridge browserBridge;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly DataStore dataStore;

        private BaseVariable<bool> connectWalletModalVisible => dataStore.HUDs.connectWalletModalVisible;
        private BaseVariable<bool> closedWalletModal => dataStore.HUDs.closedWalletModal;

        public ConnectWalletComponentController(
            IConnectWalletComponentView connectWalletView,
            IBrowserBridge browserBridge,
            IUserProfileBridge userProfileBridge,
            DataStore dataStore)
        {
            this.connectWalletView = connectWalletView;
            this.browserBridge = browserBridge;
            this.userProfileBridge = userProfileBridge;
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
            closedWalletModal.Set(true);
        }

        private void OnConfirmWalletConnection()
        {
            connectWalletView.Hide();
            connectWalletModalVisible.Set(newValue: false, notifyEvent: false);
            userProfileBridge.SignUp();
        }

        private void OnConfirmHelp() => browserBridge.OpenUrl(HELP_URL);

        private void OnChangeVisibility(bool isVisible, bool previousIsVisible)
        {
            if (isVisible)
                connectWalletView.Show();
            else
                connectWalletView.Hide();
        }
    }
}
