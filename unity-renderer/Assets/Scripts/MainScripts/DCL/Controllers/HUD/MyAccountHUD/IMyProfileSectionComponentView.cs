using System;
using UnityEngine;

namespace DCL.MyAccount
{
    public interface IMyProfileSectionComponentView
    {
        event Action<string, bool> OnCurrentNameChanged;
        event Action OnClaimNameClicked;

        void SetAsFullScreenMenuMode(Transform parentTransform);
        void SetClaimedNameMode(bool isClaimed);
        void SetCurrentName(string newName);
        void SetClaimBannerActive(bool isActive);
        void SetClaimedModeAsInput(bool isInput);
    }
}
