using System;
using UnityEngine;

namespace DCL.Wallet
{
    public interface IWalletSectionHUDComponentView
    {
        event Action OnCopyWalletAddress;
        event Action OnBuyManaClicked;
        event Action OnLearnMoreClicked;

        void SetAsFullScreenMenuMode(Transform parentTransform);
        void SetWalletAddress(string fullWalletAddress);
        void SetEthereumManaLoadingActive(bool isActive);
        void SetEthereumManaBalance(double balance);
        void SetPolygonManaLoadingActive(bool isActive);
        void SetPolygonManaBalance(double balance);
    }
}
