using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DCL.MyAccount
{
    public class EmailNotificationsComponentView : BaseComponentView, IEmailNotificationsComponentView, IPointerClickHandler
    {
        [SerializeField] internal GameObject mainContainer;
        [SerializeField] internal GameObject loadingContainer;
        [SerializeField] internal TMP_InputField emailInputField;
        [SerializeField] internal GameObject emailEditionLogoContainer;
        [SerializeField] internal GameObject emailEditionLogo;
        [SerializeField] internal GameObject emailEditionLoadingSpinner;
        [SerializeField] internal GameObject emailInputFieldInvalid;
        [SerializeField] internal GameObject emailInputFieldEditing;
        [SerializeField] internal GameObject emailInputInvalidLabel;
        [SerializeField] internal GameObject pendingStatusWarning;
        [SerializeField] internal TMP_Text pendingStatusWarningText;
        [SerializeField] internal Sprite deleteEmailSprite;
        [SerializeField] internal Sprite updateEmailSprite;

        public event Action<string> OnEmailEdited;
        public event Action<string> OnEmailSubmitted;
        public event Action OnReSendConfirmationEmailClicked;

        public Sprite deleteEmailLogo => deleteEmailSprite;
        public Sprite updateEmailLogo => updateEmailSprite;

        public override void Awake()
        {
            base.Awake();

            emailInputField.onValueChanged.AddListener(OnEmailTextChanged);
            emailInputField.onSelect.AddListener(_ =>
            {
                emailEditionLogo.SetActive(false);
                emailInputFieldEditing.SetActive(true);
            });
            emailInputField.onDeselect.AddListener(OnEmailTextSubmitted);
            emailInputField.onSubmit.AddListener(OnEmailTextSubmitted);
        }

        public override void RefreshControl() { }

        public void SetLoadingActive(bool isActive)
        {
            loadingContainer.SetActive(isActive);
            mainContainer.SetActive(!isActive);
        }

        public void SetEmail(string email) =>
            emailInputField.text = email;

        public void SetStatusAsPending(bool isPending) =>
            pendingStatusWarning.SetActive(isPending);

        public void SetEmailFormValid(bool isValid)
        {
            emailInputFieldInvalid.SetActive(!isValid);
            emailInputInvalidLabel.SetActive(!isValid);
        }

        public void ResetForm()
        {
            emailEditionLogo.SetActive(true);
            emailInputFieldEditing.SetActive(false);
            emailInputFieldInvalid.SetActive(false);
            emailInputInvalidLabel.SetActive(false);
            SetStatusAsPending(false);
        }

        public void SetEmailInputInteractable(bool isInteractable) =>
            emailInputField.interactable = isInteractable;

        public void SetEmailUpdateLoadingActive(bool isActive)
        {
            emailEditionLogoContainer.SetActive(!isActive);
            emailEditionLoadingSpinner.SetActive(isActive);
        }

        public override void Show(bool instant = false) =>
            gameObject.SetActive(true);

        public override void Hide(bool instant = false) =>
            gameObject.SetActive(false);

        public void OnPointerClick(PointerEventData eventData)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(pendingStatusWarningText, Input.mousePosition, null);
            if (linkIndex == -1)
                return;

            TMP_LinkInfo linkInfo = pendingStatusWarningText.textInfo.linkInfo[linkIndex];
            if (linkInfo.GetLinkID() == "reSendConfirmationEmail")
                OnReSendConfirmationEmailClicked?.Invoke();
        }

        public override void Dispose()
        {
            emailInputField.onValueChanged.RemoveAllListeners();
            emailInputField.onDeselect.RemoveAllListeners();
            emailInputField.onSubmit.RemoveAllListeners();

            base.Dispose();
        }

        private void OnEmailTextChanged(string newName)
        {
            OnEmailEdited?.Invoke(newName);
        }

        private void OnEmailTextSubmitted(string newName)
        {
            emailEditionLogo.SetActive(true);
            emailInputFieldEditing.SetActive(false);
            OnEmailSubmitted?.Invoke(newName);
        }
    }
}
