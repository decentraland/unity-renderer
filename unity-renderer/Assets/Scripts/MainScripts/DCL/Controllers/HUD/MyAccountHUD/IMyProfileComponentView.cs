using System;
using UnityEngine;

namespace DCL.MyAccount
{
    public interface IMyProfileComponentView
    {
        event Action<string, bool> OnCurrentNameChanged;
        event Action OnClaimNameClicked;

        void SetClaimedNameMode(bool isClaimed);
        void SetCurrentName(string newName);
        void SetClaimBannerActive(bool isActive);
        void SetClaimedModeAsInput(bool isInput);
    }
}
