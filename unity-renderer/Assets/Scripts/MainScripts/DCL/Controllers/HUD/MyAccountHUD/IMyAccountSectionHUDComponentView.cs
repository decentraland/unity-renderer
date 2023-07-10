using UnityEngine;

namespace DCL.MyAccount
{
    public interface IMyAccountSectionHUDComponentView
    {
        void SetAsFullScreenMenuMode(Transform parentTransform);
        void ShowAccountSettingsUpdatedToast();
    }
}
