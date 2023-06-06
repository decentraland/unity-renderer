using UnityEngine;

namespace DCL.Wallet
{
    public interface IWalletSectionHUDComponentView
    {
        void Show(bool instant = false);
        void Hide(bool instant = false);
        void SetAsFullScreenMenuMode(Transform parentTransform);
    }
}
