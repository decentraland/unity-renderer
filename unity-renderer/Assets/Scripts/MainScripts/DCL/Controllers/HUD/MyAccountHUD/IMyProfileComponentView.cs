using System;
using System.Collections.Generic;

namespace DCL.MyAccount
{
    public interface IMyProfileComponentView
    {
        event Action<string> OnCurrentNameEdited;
        event Action<string, bool> OnCurrentNameSubmitted;
        event Action OnGoFromClaimedToNonClaimNameClicked;
        event Action OnClaimNameClicked;
        event Action<(string title, string url)> OnLinkAdded;
        event Action<(string title, string url)> OnLinkRemoved;

        void SetClaimedNameMode(bool isClaimed);
        void SetCurrentName(string newName, string nonClaimedHashtag);
        void SetClaimBannerActive(bool isActive);
        void SetClaimedModeAsInput(bool isInput, bool cleanInputField = false);
        void SetClaimedNameDropdownOptions(List<string> claimedNamesList);
        void SetLoadingActive(bool isActive);
        void SetNonValidNameWarningActive(bool isActive);
        void SetLinks(List<(string title, string url)> links);
    }
}
