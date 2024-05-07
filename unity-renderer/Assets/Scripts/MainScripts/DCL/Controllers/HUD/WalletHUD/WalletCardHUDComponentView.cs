using System;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Wallet
{
    public class WalletCardHUDComponentView : BaseComponentView<WalletCardHUDModel>, IWalletCardHUDComponentView
    {
        [SerializeField] internal Button guestButton;
        [SerializeField] internal Button signedInWalletButton;
        [SerializeField] internal GameObject ethereumManaBalanceLoading;
        [SerializeField] internal TMP_Text ethereumManaBalanceText;
        [SerializeField] internal GameObject polygonManaBalanceLoading;
        [SerializeField] internal TMP_Text polygonManaBalanceText;

        public event Action OnWalletCardClicked;

        public override void Awake()
        {
            base.Awake();

            guestButton.onClick.AddListener(() => OnWalletCardClicked?.Invoke());
            signedInWalletButton.onClick.AddListener(() => OnWalletCardClicked?.Invoke());
        }

        public override void Dispose()
        {
            guestButton.onClick.RemoveAllListeners();
            signedInWalletButton.onClick.RemoveAllListeners();

            base.Dispose();
        }

        public override void RefreshControl()
        {
            SetWalletCardAsGuest(model.IsGuest);
            SetEthereumManaBalance(model.EthereumManaBalance);
            SetPolygonManaBalance(model.PolygonManaBalance);
        }

        public void SetWalletCardActive(bool isActive) =>
            gameObject.SetActive(isActive);

        public void SetWalletCardAsGuest(bool isGuest)
        {
            model.IsGuest = isGuest;
            guestButton.gameObject.SetActive(isGuest);
            signedInWalletButton.gameObject.SetActive(!isGuest);
        }

        public void SetEthereumManaLoadingActive(bool isActive)
        {
            ethereumManaBalanceText.gameObject.SetActive(!isActive);
            ethereumManaBalanceLoading.SetActive(isActive);
        }

        public void SetEthereumManaBalance(double balance)
        {
            model.EthereumManaBalance = balance;
            ethereumManaBalanceText.text = WalletUtils.FormatBalanceToString(balance);
        }

        public void SetPolygonManaLoadingActive(bool isActive)
        {
            polygonManaBalanceText.gameObject.SetActive(!isActive);
            polygonManaBalanceLoading.SetActive(isActive);
        }

        public void SetPolygonManaBalance(double balance)
        {
            model.PolygonManaBalance = balance;
            polygonManaBalanceText.text = WalletUtils.FormatBalanceToString(balance);
        }
    }
}
