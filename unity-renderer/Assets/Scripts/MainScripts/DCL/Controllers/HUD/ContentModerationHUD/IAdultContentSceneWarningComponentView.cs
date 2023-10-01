using System;

namespace DCL.ContentModeration
{
    public interface IAdultContentSceneWarningComponentView
    {
        event Action OnGoToSettingsClicked;

        void ShowModal();
        void HideModal();
        void SetRestrictedMode(bool isRestricted);
    }
}
