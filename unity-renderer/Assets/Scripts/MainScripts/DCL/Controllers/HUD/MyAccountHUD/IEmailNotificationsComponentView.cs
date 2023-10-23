using System;

namespace DCL.MyAccount
{
    public interface IEmailNotificationsComponentView
    {
        event Action<string> OnEmailEdited;
        public event Action<string> OnEmailSubmitted;

        void SetLoadingActive(bool isActive);
        void SetEmail(string email);
        void SetStatusAsPending(bool isPending);
        void SetEmailFormValid(bool isValid);
        void ResetForm();
    }
}
