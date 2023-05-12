
using System;
using UnityEngine;

public interface IProfileHUDView
{
    event EventHandler SignedUpPressed;
    event EventHandler LogedOutPressed;
    event EventHandler ClaimNamePressed;

    event EventHandler Opened;
    event EventHandler Closed;
    event EventHandler<string> NameSubmitted;
    event EventHandler<string> DescriptionSubmitted;

    event EventHandler ManaInfoPressed;
    event EventHandler ManaPurchasePressed;

    event EventHandler TermsAndServicesPressed;
    event EventHandler PrivacyPolicyPressed;

    GameObject GameObject { get; }
    RectTransform ExpandedMenu { get; }
    RectTransform TutorialReference { get; }


    bool HasManaCounterView();
    bool HasPolygonManaCounterView();
    bool IsDesciptionIsLongerThanMaxCharacters();

    void SetProfile(UserProfile profile);
    void SetManaBalance(string balance);
    void SetPolygonBalance(double balance);
    void SetWalletSectionEnabled(bool isEnabled);
    void SetNonWalletSectionEnabled(bool isEnabled);
    void SetStartMenuButtonActive(bool isActive);

    void SetDescriptionIsEditing(bool isEditing);
    void ShowProfileIcon(bool show);
    void ShowExpanded(bool show);
}
