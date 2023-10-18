using System;

namespace DCL.MyAccount
{
    public interface IEmailNotificationsComponentView
    {
        event Action<string> OnEmailEdited;
        event Action<bool> OnSubscribeToNewsletterEdited;

        void SetEmail(string email);
        void SetSubscribeToNewsletter(bool isSubscribed);
    }
}
