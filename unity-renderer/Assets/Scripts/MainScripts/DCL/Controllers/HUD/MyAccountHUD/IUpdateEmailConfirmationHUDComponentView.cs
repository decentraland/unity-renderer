using System;
using UnityEngine;

namespace DCL.MyAccount
{
    public interface IUpdateEmailConfirmationHUDComponentView
    {
        event Action<bool> OnConfirmationModalAccepted;

        void ShowConfirmationModal(Sprite actionLogo, string text);
        void HideConfirmationModal();
    }
}
