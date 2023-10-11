using System;

namespace DCL.ContentModeration
{
    public interface IAdultContentAgeConfirmationComponentView
    {
        event Action OnCancelClicked;
        event Action OnConfirmClicked;

        void ShowModal();
    }
}
