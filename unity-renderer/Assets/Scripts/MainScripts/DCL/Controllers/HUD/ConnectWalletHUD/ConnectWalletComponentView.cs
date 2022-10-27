using System;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Guests.HUD.ConnectWallet
{
    public class ConnectWalletComponentView : BaseComponentView, IConnectWalletComponentView
    {
        [SerializeField] internal Button backgroundButton;
        [SerializeField] internal Button closeButton;
        [SerializeField] internal ButtonComponentView connectButton;
        [SerializeField] internal ButtonComponentView helpButton;

        public event Action OnCancel;
        public event Action OnConnect;
        public event Action OnHelp;

        public override void Awake()
        {
            base.Awake();

            backgroundButton.onClick.AddListener(() => OnCancel?.Invoke());
            closeButton.onClick.AddListener(() => OnCancel?.Invoke());
            connectButton.onClick.AddListener(() => OnConnect?.Invoke());
            helpButton.onClick.AddListener(() => OnHelp?.Invoke());
        }

        public override void Dispose()
        {
            backgroundButton.onClick.RemoveAllListeners();
            closeButton.onClick.RemoveAllListeners();
            connectButton.onClick.RemoveAllListeners();
            helpButton.onClick.RemoveAllListeners();

            base.Dispose();
        }

        public override void RefreshControl() { }

        internal static ConnectWalletComponentView Create()
        {
            ConnectWalletComponentView connectWalletComponenView = Instantiate(Resources.Load<GameObject>("ConnectWalletHUD")).GetComponent<ConnectWalletComponentView>();
            connectWalletComponenView.name = "_ConnectWalletHUD";

            return connectWalletComponenView;
        }
    }
}