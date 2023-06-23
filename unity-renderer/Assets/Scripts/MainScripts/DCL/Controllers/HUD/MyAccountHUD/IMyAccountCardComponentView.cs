using System;

namespace DCL.MyAccount
{
    public interface IMyAccountCardComponentView
    {
        event Action OnPreviewProfileClicked;
        event Action OnAccountSettingsClicked;
        event Action OnSignOutClicked;
        event Action OnTermsOfServiceClicked;
        event Action OnPrivacyPolicyClicked;

        void Show(bool instant = false);
        void Hide(bool instant = false);
        void SetSignOutButtonActive(bool isActive);
    }
}
