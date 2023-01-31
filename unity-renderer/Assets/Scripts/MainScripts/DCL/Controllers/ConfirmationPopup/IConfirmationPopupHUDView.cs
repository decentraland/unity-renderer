using System;
using UIComponents.Scripts.Components;

namespace DCL.ConfirmationPopup
{
    public interface IConfirmationPopupHUDView : IBaseComponentView<ConfirmationPopupHUDViewModel>
    {
        event Action OnConfirm;
        event Action OnCancel;
    }
}
