using System;

namespace DCL.ContentModeration
{
    public interface IAdultContentSceneWarningComponentView
    {
        event Action OnLearnMoreClicked;
        event Action OnGoToSettingsClicked;

        void ShowModal();
        void HideModal();
    }
}
