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
        event Action<string> OnAboutDescriptionSubmitted;
        event Action<(string title, string url)> OnLinkAdded;
        event Action<(string title, string url)> OnLinkRemoved;

        void SetClaimedNameMode(bool isClaimed);
        void SetCurrentName(string newName, string nonClaimedHashtag);
        void SetClaimNameBannerActive(bool isActive);
        void SetClaimedNameModeAsInput(bool isInput, bool cleanInputField = false);
        void SetClaimedNameDropdownOptions(List<string> claimedNamesList);
        void SetLoadingActive(bool isActive);
        void SetNonValidNameWarningActive(bool isActive);
        void SetAboutDescription(string newDesc);
        void SetAboutEnabled(bool isEnabled);
        void SetLinks(List<UserProfileModel.Link> links);
        void ClearLinkInput();
        void EnableOrDisableAddLinksOption(bool enabled);
        void SetLinksEnabled(bool isEnabled);
        void SetAdditionalInfoOptions(AdditionalInfoOptionsModel model);
        void SetAdditionalInfoValues(Dictionary<string, (string title, string value)> values);
        void SetAdditionalInfoEnabled(bool isEnabled);
        void RefreshContentLayout();
    }
}
