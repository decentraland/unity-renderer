using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.MyAccount
{
    public class UpdateEmailConfirmationHUDComponentView : BaseComponentView, IUpdateEmailConfirmationHUDComponentView
    {
        [SerializeField] internal Image actionLogoImage;
        [SerializeField] internal TMP_Text confirmationText;
        [SerializeField] internal Button backgroundButton;
        [SerializeField] internal Button noButton;
        [SerializeField] internal Button yesButton;

        public event Action<bool> OnConfirmationModalAccepted;

        public override void Awake()
        {
            base.Awake();

            backgroundButton.onClick.AddListener(RejectConfirmation);

            noButton.onClick.AddListener(RejectConfirmation);
            yesButton.onClick.AddListener(AcceptConfirmation);
        }

        public override void Dispose()
        {
            noButton.onClick.RemoveAllListeners();
            yesButton.onClick.RemoveAllListeners();
            base.Dispose();
        }

        public override void RefreshControl() { }

        public void ShowConfirmationModal(Sprite actionLogo, string text)
        {
            actionLogoImage.sprite = actionLogo;
            confirmationText.text = text;
            Show();
        }

        public void HideConfirmationModal() =>
            Hide();

        private void AcceptConfirmation()
        {
            HideConfirmationModal();
            OnConfirmationModalAccepted?.Invoke(true);
        }

        private void RejectConfirmation()
        {
            HideConfirmationModal();
            OnConfirmationModalAccepted?.Invoke(false);
        }
    }
}
