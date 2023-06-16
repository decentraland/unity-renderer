using System;

namespace DCL.Wallet
{
    public interface IWalletCardHUDComponentView
    {
        event Action OnWalletCardClicked;

        void SetWalletCardActive(bool isActive);
        void SetWalletCardAsGuest(bool isGuest);
        void SetEthereumManaLoadingActive(bool isActive);
        void SetEthereumManaBalance(double balance);
        void SetPolygonManaLoadingActive(bool isActive);
        void SetPolygonManaBalance(double balance);
    }
}
