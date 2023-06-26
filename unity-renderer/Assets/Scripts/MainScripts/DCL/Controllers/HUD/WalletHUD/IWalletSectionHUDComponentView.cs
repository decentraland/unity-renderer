using DCL.Guests.HUD.ConnectWallet;
using System;
using UnityEngine;

namespace DCL.Wallet
{
    public interface IWalletSectionHUDComponentView
    {
        event Action OnCopyWalletAddress;
        event Action<bool> OnBuyManaClicked;
        event Action OnLearnMoreClicked;

        IConnectWalletComponentView currentConnectWalletView { get; }

        void SetAsFullScreenMenuMode(Transform parentTransform);
        void SetWalletSectionAsGuest(bool isGuest);
        void SetWalletAddress(string fullWalletAddress);
        void SetEthereumManaLoadingActive(bool isActive);
        void SetEthereumManaBalance(double balance);
        void SetPolygonManaLoadingActive(bool isActive);
        void SetPolygonManaBalance(double balance);
    }
}
