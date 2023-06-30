using DCL;
using DCL.MyAccount;
using ExploreV2Analytics;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Environment = DCL.Environment;

public class ProfileHUDViewV2 : BaseComponentView, IProfileHUDView
{
    private const float COPY_TOAST_VISIBLE_TIME = 3;
    private const int ADDRESS_CHUNK_LENGTH = 6;
    private const int NAME_POSTFIX_LENGTH = 4;
    private const string OPEN_PASSPORT_SOURCE = "ProfileHUD";

    [SerializeField] private MyAccountCardComponentView myAccountCardView;
    [SerializeField] private RectTransform myAccountCardLayout;
    [SerializeField] private RectTransform mainRootLayout;
    [SerializeField] internal GameObject loadingSpinner;
    [SerializeField] internal ShowHideAnimator copyToast;
    [SerializeField] internal GameObject copyTooltip;
    [SerializeField] private GameObject expandedObject;
    [SerializeField] private GameObject profilePicObject;
    [SerializeField] internal InputAction_Trigger closeAction;
    [SerializeField] internal Canvas mainCanvas;
    [SerializeField] internal Button viewPassportButton;

    [Header("Hide GOs on claimed name")]
    [SerializeField]
    internal GameObject[] hideOnNameClaimed;

    [Header("Connected wallet sections")]
    [SerializeField]
    internal GameObject connectedWalletSection;

    [SerializeField]
    internal GameObject nonConnectedWalletSection;

    [Header("Thumbnail")]
    [SerializeField]
    internal RawImage imageAvatarThumbnail;

    [SerializeField]
    protected internal Button buttonToggleMenu;

    [Header("Texts")]
    [SerializeField] internal TextMeshProUGUI textName;
    [SerializeField] internal TextMeshProUGUI textPostfix;
    [SerializeField] internal TextMeshProUGUI textAddress;

    [Header("Buttons")]
    [SerializeField] protected internal Button buttonClaimName;
    [SerializeField] protected internal Button buttonCopyAddress;
    [SerializeField] protected internal Button buttonLogOut;
    [SerializeField] protected internal Button buttonSignUp;
    [SerializeField] protected internal Button_OnPointerDown buttonTermsOfServiceForConnectedWallets;
    [SerializeField] protected internal Button_OnPointerDown buttonPrivacyPolicyForConnectedWallets;
    [SerializeField] protected internal Button_OnPointerDown buttonTermsOfServiceForNonConnectedWallets;
    [SerializeField] protected internal Button_OnPointerDown buttonPrivacyPolicyForNonConnectedWallets;

    [Header("Name Edition")]
    [SerializeField] protected internal Button_OnPointerDown buttonEditName;
    [SerializeField] protected internal Button_OnPointerDown buttonEditNamePrefix;
    [SerializeField] internal TMP_InputField inputName;
    [SerializeField] internal TextMeshProUGUI textCharLimit;
    [SerializeField] internal ManaCounterView manaCounterView;
    [SerializeField] internal ManaCounterView polygonManaCounterView;

    [Header("Tutorial Config")]
    [SerializeField] internal RectTransform tutorialTooltipReference;

    [Header("Description")]
    [SerializeField] internal GameObject charLimitDescriptionContainer;
    [SerializeField] internal GameObject descriptionStartEditingGo;
    [SerializeField] internal GameObject descriptionIsEditingGo;
    [SerializeField] internal Button descriptionStartEditingButton;
    [SerializeField] internal Button descriptionIsEditingButton;
    [SerializeField] internal TMP_InputField descriptionInputText;
    [SerializeField] internal TextMeshProUGUI textCharLimitDescription;

    [SerializeField] internal GameObject descriptionContainer;

    private InputAction_Trigger.Triggered closeActionDelegate;

    private Coroutine copyToastRoutine;
    private UserProfile profile;
    private string description;
    private string userId;
    private BaseVariable<(string playerId, string source)> currentPlayerId;

    public event EventHandler ClaimNamePressed;
    public event EventHandler SignedUpPressed;
    public event EventHandler LogedOutPressed;
    public event EventHandler Opened;
    public event EventHandler Closed;
    public event EventHandler<string> NameSubmitted;
    public event EventHandler<string> DescriptionSubmitted;
    public event EventHandler ManaInfoPressed;
    public event EventHandler ManaPurchasePressed;
    public event EventHandler TermsAndServicesPressed;
    public event EventHandler PrivacyPolicyPressed;

    internal bool isStartMenuInitialized = false;

    private HUDCanvasCameraModeController hudCanvasCameraModeController;
    public GameObject GameObject => gameObject;
    public RectTransform ExpandedMenu => mainRootLayout;
    public RectTransform MyAccountCardLayout => myAccountCardLayout;
    public RectTransform MyAccountCardMenu => (RectTransform)myAccountCardView.transform;
    public MyAccountCardComponentView MyAccountCardView => myAccountCardView;
    public RectTransform TutorialReference => tutorialTooltipReference;

    public override void Awake()
    {
        currentPlayerId = DataStore.i.HUDs.currentPlayerId;

        buttonLogOut.onClick.AddListener(() => LogedOutPressed?.Invoke(this, EventArgs.Empty));
        buttonSignUp.onClick.AddListener(() => SignedUpPressed?.Invoke(this, EventArgs.Empty));
        buttonClaimName.onClick.AddListener(() => ClaimNamePressed?.Invoke(this, EventArgs.Empty));

        buttonTermsOfServiceForConnectedWallets.onClick.AddListener(() => TermsAndServicesPressed?.Invoke(this, EventArgs.Empty));
        buttonTermsOfServiceForNonConnectedWallets.onClick.AddListener(() => TermsAndServicesPressed?.Invoke(this, EventArgs.Empty));
        buttonPrivacyPolicyForConnectedWallets.onClick.AddListener(() => PrivacyPolicyPressed?.Invoke(this, EventArgs.Empty));
        buttonPrivacyPolicyForNonConnectedWallets.onClick.AddListener(() => PrivacyPolicyPressed?.Invoke(this, EventArgs.Empty));

        manaCounterView.buttonManaInfo.onClick.AddListener(() => ManaInfoPressed?.Invoke(this, EventArgs.Empty));
        polygonManaCounterView.buttonManaInfo.onClick.AddListener(() => ManaInfoPressed?.Invoke(this, EventArgs.Empty));
        manaCounterView.buttonManaPurchase.onClick.AddListener(() => ManaPurchasePressed?.Invoke(this, EventArgs.Empty));
        polygonManaCounterView.buttonManaPurchase.onClick.AddListener(() => ManaPurchasePressed?.Invoke(this, EventArgs.Empty));

        closeActionDelegate = (x) => Hide();

        buttonToggleMenu.onClick.AddListener(OpenStartMenu);
        buttonCopyAddress.onClick.AddListener(CopyAddress);
        buttonEditName.onPointerDown += () => ActivateProfileNameEditionMode(true);
        buttonEditNamePrefix.onPointerDown += () => ActivateProfileNameEditionMode(true);
        inputName.onValueChanged.AddListener(UpdateNameCharLimit);
        inputName.onDeselect.AddListener((newName) =>
        {
            ActivateProfileNameEditionMode(false);
            NameSubmitted?.Invoke(this, newName);
        });

        descriptionStartEditingButton.onClick.AddListener(descriptionInputText.Select);
        descriptionIsEditingButton.onClick.AddListener(() => descriptionInputText.OnDeselect(null));
        descriptionInputText.onTextSelection.AddListener((description, x, y) =>
        {
            descriptionInputText.text = profile.description;
            SetDescriptionIsEditing(true);
            UpdateDescriptionCharLimit(description);
        });
        descriptionInputText.onSelect.AddListener(x =>
        {
            descriptionInputText.text = profile.description;
            SetDescriptionIsEditing(true);
            UpdateDescriptionCharLimit(description);
        });
        descriptionInputText.onValueChanged.AddListener(UpdateDescriptionCharLimit);
        descriptionInputText.onDeselect.AddListener(description =>
        {
            this.description = description;
            DescriptionSubmitted?.Invoke(this, description);

            SetDescriptionIsEditing(false);
            UpdateLinksInformation(description);
        });
        SetDescriptionIsEditing(false);

        copyToast.gameObject.SetActive(false);
        hudCanvasCameraModeController = new HUDCanvasCameraModeController(GetComponent<Canvas>(), DataStore.i.camera.hudsCamera);

        viewPassportButton.onClick.RemoveAllListeners();
        viewPassportButton.onClick.AddListener(OpenPassport);
        Show(false);
    }

    public bool HasManaCounterView() => manaCounterView != null;
    public bool HasPolygonManaCounterView() => polygonManaCounterView != null;
    public bool IsDesciptionIsLongerThanMaxCharacters() => descriptionInputText.characterLimit < descriptionInputText.text.Length;
    public void SetManaBalance(string balance) => manaCounterView.SetBalance(balance);
    public void SetPolygonBalance(double balance) => polygonManaCounterView.SetBalance(balance);
    public void SetWalletSectionEnabled(bool isEnabled) => connectedWalletSection.SetActive(isEnabled);
    public void SetNonWalletSectionEnabled(bool isEnabled) => nonConnectedWalletSection.SetActive(isEnabled);
    public void SetStartMenuButtonActive(bool isActive) => isStartMenuInitialized = isActive;
    public override void RefreshControl() { }

    public void SetProfile(UserProfile userProfile)
    {
        UpdateLayoutByProfile(userProfile);
        if (profile == null)
        {
            description = userProfile.description;
            descriptionInputText.text = description;
            UpdateLinksInformation(description);
        }

        profile = userProfile;
    }

    public void ShowProfileIcon(bool show)
    {
        profilePicObject.SetActive(show);
    }

    public void ShowExpanded(bool show, bool showMyAccountVersion = false)
    {
        if (!show)
        {
            expandedObject.SetActive(false);
            myAccountCardView.Hide();
        }
        else
        {
            if (!showMyAccountVersion)
                expandedObject.SetActive(true);
            else
                myAccountCardView.Show();
        }

        if (show && profile && !showMyAccountVersion)
            UpdateLayoutByProfile(profile);
    }

    public void SetDescriptionIsEditing(bool isEditing)
    {
        SetDescriptionCharLimitEnabled(isEditing);
        descriptionStartEditingGo.SetActive(!isEditing);
        descriptionIsEditingGo.SetActive(isEditing);
    }

    private void UpdateLayoutByProfile(UserProfile userProfile)
    {
        if (userProfile.hasClaimedName)
            HandleClaimedProfileName(userProfile);
        else
            HandleUnverifiedProfileName(userProfile);

        SetConnectedWalletSectionActive(userProfile.hasConnectedWeb3);
        HandleProfileAddress(userProfile);
        HandleProfileSnapshot(userProfile);
        SetDescriptionEnabled(userProfile.hasConnectedWeb3);
        SetUserId(userProfile);

        string[] nameSplits = userProfile.userName.Split('#');
        if (nameSplits.Length >= 2)
        {
            textName.text = nameSplits[0];
            textPostfix.text = $"#{nameSplits[1]};";
        }
        else
        {
            textName.text = userProfile.userName;
            textPostfix.text = userProfile.userId;
        }
    }

    private void SetUserId(UserProfile userProfile)
    {
        userId = userProfile.userId;
    }

    private void OpenStartMenu()
    {
        if (isStartMenuInitialized)
        {
            if (!DataStore.i.exploreV2.isOpen.Get())
            {
                var exploreV2Analytics = new ExploreV2Analytics.ExploreV2Analytics();
                exploreV2Analytics.SendStartMenuVisibility(
                    true,
                    ExploreUIVisibilityMethod.FromClick);
            }
            DataStore.i.exploreV2.isOpen.Set(true);
        }
    }

    private void ActivateProfileNameEditionMode(bool activate)
    {
        if (profile != null && profile.hasClaimedName)
            return;

        textName.gameObject.SetActive(!activate);
        inputName.gameObject.SetActive(activate);

        if (activate)
        {
            inputName.text = textName.text;
            inputName.Select();
        }
    }

    private void SetDescriptionCharLimitEnabled(bool enabled)
    {
        charLimitDescriptionContainer.SetActive(enabled);
    }

    private void OpenPassport()
    {
        if (string.IsNullOrEmpty(userId))
            return;

        ShowExpanded(false);
        currentPlayerId.Set((userId, OPEN_PASSPORT_SOURCE));
    }

    private void HandleProfileSnapshot(UserProfile userProfile)
    {
        loadingSpinner.SetActive(true);
        userProfile.snapshotObserver.AddListener(SetProfileImage);
    }

    private void HandleClaimedProfileName(UserProfile userProfile)
    {
        textName.text = userProfile.userName;
        SetActiveUnverifiedNameGOs(false);
    }

    private void HandleUnverifiedProfileName(UserProfile userProfile)
    {
        textName.text = string.IsNullOrEmpty(userProfile.userName) || userProfile.userName.Length <= NAME_POSTFIX_LENGTH
            ? userProfile.userName
            : userProfile.userName[..(userProfile.userName.Length - NAME_POSTFIX_LENGTH - 1)];

        textPostfix.text = $"#{userProfile.userId[^NAME_POSTFIX_LENGTH..]}";
        SetActiveUnverifiedNameGOs(true);
    }

    private void SetConnectedWalletSectionActive(bool active)
    {
        connectedWalletSection.SetActive(active);
        nonConnectedWalletSection.SetActive(!active);
        buttonLogOut.gameObject.SetActive(active);
    }

    private void SetActiveUnverifiedNameGOs(bool active)
    {
        for (int i = 0; i < hideOnNameClaimed.Length; i++)
            hideOnNameClaimed[i].SetActive(active);
    }

    private void HandleProfileAddress(UserProfile userProfile)
    {
        string address = userProfile.userId;
        string start = address.Substring(0, ADDRESS_CHUNK_LENGTH);
        string end = address.Substring(address.Length - ADDRESS_CHUNK_LENGTH);
        textAddress.text = $"{start}...{end}";
    }

    private void SetProfileImage(Texture2D texture)
    {
        loadingSpinner.SetActive(false);
        imageAvatarThumbnail.texture = texture;
    }

    private void OnDestroy()
    {
        hudCanvasCameraModeController?.Dispose();
        if (profile)
            profile.snapshotObserver.RemoveListener(SetProfileImage);
    }

    private void CopyAddress()
    {
        if (!profile)
            return;

        Environment.i.platform.clipboard.WriteText(profile.userId);

        copyTooltip.gameObject.SetActive(false);
        if (copyToastRoutine != null)
            StopCoroutine(copyToastRoutine);

        copyToastRoutine = StartCoroutine(ShowCopyToast());
    }

    private void OnEnable() { closeAction.OnTriggered += closeActionDelegate; }

    private void OnDisable() { closeAction.OnTriggered -= closeActionDelegate; }

    private void UpdateNameCharLimit(string newValue) { textCharLimit.text = $"{newValue.Length}/{inputName.characterLimit}"; }

    private void SetDescriptionEnabled(bool enabled)
    {
        descriptionContainer.SetActive(enabled);
    }

    private void UpdateDescriptionCharLimit(string description) =>
        textCharLimitDescription.text = $"{description.Length}/{descriptionInputText.characterLimit}";

    private void UpdateLinksInformation(string description)
    {
        string[] words = description.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        bool[] areLinks = new bool[words.Length];
        bool linksFound = false;

        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].StartsWith("[") && words[i].Contains("]") && words[i].Contains("(") && words[i].EndsWith(")"))
            {
                string link = words[i].Split("(")[0].Replace("[", "").Replace("]", "");
                string id = words[i].Split("]")[1].Replace("(", "").Replace(")", "");
                string[] elements = words[i].Split('.');
                if (elements.Length >= 2)
                {
                    words[i] = $"<color=\"blue\"><link=\"{id}\">{link}</link></color>";
                    areLinks[i] = true;
                    linksFound = true;
                }
            }
        }

        if (linksFound)
        {
            bool foundLinksAtTheEnd = false;
            for (int i = words.Length - 1; i >= 0; i--)
            {
                if (string.IsNullOrEmpty(words[i]))
                    continue;

                if (!foundLinksAtTheEnd && !areLinks[i])
                    break;

                if (areLinks[i])
                {
                    foundLinksAtTheEnd = true;
                    continue;
                }

                words[i] += "\n\n";
                break;
            }
            descriptionInputText.text = string.Join(" ", words);
        }
    }


    private IEnumerator ShowCopyToast()
    {
        if (!copyToast.gameObject.activeSelf)
            copyToast.gameObject.SetActive(true);

        copyToast.Show();
        yield return new WaitForSeconds(COPY_TOAST_VISIBLE_TIME);
        copyToast.Hide();
    }
}
