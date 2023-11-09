using System;
using UnityEngine;

namespace DCL.MyAccount
{
    public interface IEmailNotificationsComponentView
    {
        event Action<string> OnEmailEdited;
        public event Action<string> OnEmailSubmitted;
        public event Action OnReSendConfirmationEmailClicked;

        Sprite deleteEmailLogo { get; }
        Sprite updateEmailLogo { get; }

        void SetLoadingActive(bool isActive);
        void SetEmail(string email);
        void SetStatusAsPending(bool isPending);
        void SetEmailFormValid(bool isValid);
        void ResetForm();
        void SetEmailInputInteractable(bool isInteractable);
        void SetEmailUpdateLoadingActive(bool isActive);
    }
}
