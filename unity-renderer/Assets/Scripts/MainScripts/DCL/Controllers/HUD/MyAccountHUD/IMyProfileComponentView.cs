﻿using System;
using System.Collections.Generic;

namespace DCL.MyAccount
{
    public interface IMyProfileComponentView
    {
        event Action<string> OnCurrentNameEdited;
        event Action<string, bool> OnCurrentNameSubmitted;
        event Action OnClaimNameClicked;

        void SetClaimedNameMode(bool isClaimed);
        void SetCurrentName(string newName);
        void SetClaimBannerActive(bool isActive);
        void SetClaimedModeAsInput(bool isInput, bool cleanInputField = false);
        void SetClaimedNameDropdownOptions(List<string> claimedNamesList);
        void SetLoadingActive(bool isActive);
    }
}
