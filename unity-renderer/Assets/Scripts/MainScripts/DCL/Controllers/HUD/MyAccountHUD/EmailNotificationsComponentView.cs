using System;
using TMPro;
using UnityEngine;

namespace DCL.MyAccount
{
    public class EmailNotificationsComponentView : BaseComponentView, IEmailNotificationsComponentView
    {
        [SerializeField] internal TMP_InputField emailInputField;
        [SerializeField] internal ToggleComponentView subscribeToNewsletterToggle;

        public event Action<string> OnEmailEdited;
        public event Action<bool> OnSubscribeToNewsletterEdited;

        public override void Awake()
        {
            base.Awake();

            emailInputField.onValueChanged.AddListener(OnEmailInputField);
            subscribeToNewsletterToggle.OnSelectedChanged += OnSubscribeToNewsletterToggleChanged;
        }

        public override void RefreshControl() { }

        public void SetEmail(string email)
        {
            emailInputField.text = email;
        }

        public void SetSubscribeToNewsletter(bool isSubscribed)
        {
            subscribeToNewsletterToggle.isOn = isSubscribed;
        }

        public override void Show(bool instant = false) =>
            gameObject.SetActive(true);

        public override void Hide(bool instant = false) =>
            gameObject.SetActive(false);

        public override void Dispose()
        {
            emailInputField.onValueChanged.RemoveAllListeners();
            subscribeToNewsletterToggle.OnSelectedChanged -= OnSubscribeToNewsletterToggleChanged;

            base.Dispose();
        }

        private void OnEmailInputField(string newEmail) =>
            OnEmailEdited?.Invoke(newEmail);

        private void OnSubscribeToNewsletterToggleChanged(bool isOn, string id, string text) =>
            OnSubscribeToNewsletterEdited?.Invoke(isOn);
    }
}
