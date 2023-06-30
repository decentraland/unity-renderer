using System;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.MyAccount
{
    public class MyAccountCardComponentView : BaseComponentView, IMyAccountCardComponentView
    {
        [SerializeField] private Button previewProfileButton;
        [SerializeField] private Button accountSettingsButton;
        [SerializeField] private Button signOutButton;
        [SerializeField] private Button termsOfServiceButton;
        [SerializeField] private Button privacyPolicyButton;

        public event Action OnPreviewProfileClicked;
        public event Action OnAccountSettingsClicked;
        public event Action OnSignOutClicked;
        public event Action OnTermsOfServiceClicked;
        public event Action OnPrivacyPolicyClicked;

        public override void RefreshControl() { }

        public override void Awake()
        {
            base.Awake();

            previewProfileButton.onClick.AddListener(() => OnPreviewProfileClicked?.Invoke());
            accountSettingsButton.onClick.AddListener(() => OnAccountSettingsClicked?.Invoke());
            signOutButton.onClick.AddListener(() => OnSignOutClicked?.Invoke());
            termsOfServiceButton.onClick.AddListener(() => OnTermsOfServiceClicked?.Invoke());
            privacyPolicyButton.onClick.AddListener(() => OnPrivacyPolicyClicked?.Invoke());
        }

        public override void Show(bool instant = false) =>
            gameObject.SetActive(true);

        public override void Hide(bool instant = false) =>
            gameObject.SetActive(false);

        public void SetSignOutButtonActive(bool isActive) =>
            signOutButton.gameObject.SetActive(isActive);

        public override void Dispose()
        {
            previewProfileButton.onClick.RemoveAllListeners();
            accountSettingsButton.onClick.RemoveAllListeners();
            signOutButton.onClick.RemoveAllListeners();
            termsOfServiceButton.onClick.RemoveAllListeners();
            privacyPolicyButton.onClick.RemoveAllListeners();

            base.Dispose();
        }
    }
}
