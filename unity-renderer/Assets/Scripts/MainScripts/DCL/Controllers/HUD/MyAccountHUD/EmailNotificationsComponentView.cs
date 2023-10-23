using System;
using TMPro;
using UnityEngine;

namespace DCL.MyAccount
{
    public class EmailNotificationsComponentView : BaseComponentView, IEmailNotificationsComponentView
    {
        [SerializeField] internal GameObject mainContainer;
        [SerializeField] internal GameObject loadingContainer;
        [SerializeField] internal TMP_InputField emailInputField;
        [SerializeField] internal GameObject emailEditionLogo;
        [SerializeField] internal GameObject emailInputFieldInvalid;
        [SerializeField] internal GameObject emailInputInvalidLabel;
        [SerializeField] internal GameObject pendingStatusWarning;

        public event Action<string> OnEmailEdited;
        public event Action<string> OnEmailSubmitted;

        public override void Awake()
        {
            base.Awake();

            emailInputField.onValueChanged.AddListener(OnEmailTextChanged);
            emailInputField.onSelect.AddListener(_ => emailEditionLogo.SetActive(false));
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
            emailInputFieldInvalid.SetActive(false);
            emailInputInvalidLabel.SetActive(false);
            SetStatusAsPending(false);
        }

        public override void Show(bool instant = false) =>
            gameObject.SetActive(true);

        public override void Hide(bool instant = false) =>
            gameObject.SetActive(false);

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
            OnEmailSubmitted?.Invoke(newName);
        }
    }
}
