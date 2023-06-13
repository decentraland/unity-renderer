using DCL.Guests.HUD.ConnectWallet;
using System;
using System.Collections;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DCL.Wallet
{
    public class WalletSectionHUDComponentView : BaseComponentView<WalletSectionHUDModel>, IWalletSectionHUDComponentView, IPointerClickHandler
    {
        private const float COPY_TOAST_VISIBLE_TIME = 3;
        private const string LEARN_MORE_LINK_ID = "learn_more";

        [SerializeField] internal GameObject signedInWalletContainer;
        [SerializeField] internal GameObject guestContainer;
        [SerializeField] internal ConnectWalletComponentView connectWalletView;
        [SerializeField] internal TMP_Text walletAddressText;
        [SerializeField] internal Button copyWalletAddressButton;
        [SerializeField] internal ShowHideAnimator copyWalletAddressToast;
        [SerializeField] internal TMP_Text ethereumManaLearnMoreText;
        [SerializeField] internal Button buyEthereumManaButton;
        [SerializeField] internal TMP_Text polygonManaLearnMoreText;
        [SerializeField] internal Button buyPolygonManaButton;
        [SerializeField] internal TMP_Text ethereumManaBalanceText;
        [SerializeField] internal GameObject ethereumManaBalanceLoading;
        [SerializeField] internal TMP_Text polygonManaBalanceText;
        [SerializeField] internal GameObject polygonManaBalanceLoading;

        public event Action OnCopyWalletAddress;
        public event Action OnBuyManaClicked;
        public event Action OnLearnMoreClicked;

        public IConnectWalletComponentView currentConnectWalletView => connectWalletView;

        private Transform thisTransform;
        private Coroutine copyToastRoutine;

        public override void Awake()
        {
            base.Awake();

            thisTransform = transform;

            copyWalletAddressButton.onClick.AddListener(CopyWalletAddress);
            buyEthereumManaButton.onClick.AddListener(() => OnBuyManaClicked?.Invoke());
            buyPolygonManaButton.onClick.AddListener(() => OnBuyManaClicked?.Invoke());
        }

        public override void Dispose()
        {
            copyWalletAddressButton.onClick.RemoveAllListeners();
            buyEthereumManaButton.onClick.RemoveAllListeners();
            buyPolygonManaButton.onClick.RemoveAllListeners();

            base.Dispose();
        }

        public override void RefreshControl()
        {
            SetWalletSectionAsGuest(model.IsGuest);
            SetWalletAddress(model.WalletAddress);
            SetEthereumManaBalance(model.EthereumManaBalance);
            SetPolygonManaBalance(model.PolygonManaBalance);
        }

        public void SetAsFullScreenMenuMode(Transform parentTransform)
        {
            if (parentTransform == null)
                return;

            thisTransform.SetParent(parentTransform);
            thisTransform.localScale = Vector3.one;

            RectTransform rectTransform = thisTransform as RectTransform;
            if (rectTransform == null) return;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localPosition = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
        }

        public void SetWalletSectionAsGuest(bool isGuest)
        {
            model.IsGuest = isGuest;
            signedInWalletContainer.SetActive(!isGuest);
            guestContainer.SetActive(isGuest);
        }

        public void SetWalletAddress(string fullWalletAddress)
        {
            model.WalletAddress = fullWalletAddress;
            walletAddressText.text = fullWalletAddress;
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

        public void OnPointerClick(PointerEventData eventData)
        {
            if (CheckClickOnLearnMoreLink(ethereumManaLearnMoreText))
                return;

            CheckClickOnLearnMoreLink(polygonManaLearnMoreText);
        }

        private void CopyWalletAddress()
        {
            if (copyToastRoutine != null)
                StopCoroutine(copyToastRoutine);

            copyToastRoutine = StartCoroutine(ShowCopyToast());

            OnCopyWalletAddress?.Invoke();
        }

        private IEnumerator ShowCopyToast()
        {
            if (!copyWalletAddressToast.gameObject.activeSelf)
                copyWalletAddressToast.gameObject.SetActive(true);

            copyWalletAddressToast.Show();
            yield return new WaitForSeconds(COPY_TOAST_VISIBLE_TIME);
            copyWalletAddressToast.Hide();
        }

        private bool CheckClickOnLearnMoreLink(TMP_Text textToCheck)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(textToCheck, Input.mousePosition, textToCheck.canvas.worldCamera);
            if (linkIndex == -1)
                return false;

            TMP_LinkInfo linkInfo = textToCheck.textInfo.linkInfo[linkIndex];
            if (linkInfo.GetLinkID() != LEARN_MORE_LINK_ID)
                return false;

            OnLearnMoreClicked?.Invoke();
            return true;
        }
    }
}
