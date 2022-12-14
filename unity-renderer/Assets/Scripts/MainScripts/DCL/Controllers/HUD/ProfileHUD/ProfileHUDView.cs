using DCL;
using ExploreV2Analytics;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Environment = DCL.Environment;

public class ProfileHUDView : BaseComponentView, IProfileHUDView
{
    private const int ADDRESS_CHUNK_LENGTH = 6;
    private const int NAME_POSTFIX_LENGTH = 4;
    private const float COPY_TOAST_VISIBLE_TIME = 3;

    [SerializeField] private ShowHideAnimator expandedLayoutAnimator;
    [SerializeField] private RectTransform mainRootLayout;
    [SerializeField] private GameObject loadingSpinner;
    [SerializeField] private ShowHideAnimator copyToast;
    [SerializeField] private GameObject copyTooltip;
    [SerializeField] private InputAction_Trigger closeAction;
    [SerializeField] private Canvas mainCanvas;

    [Header("Hide GOs on claimed name")]
    [SerializeField] internal GameObject[] hideOnNameClaimed;

    [Header("Connected wallet sections")]
    [SerializeField] internal GameObject connectedWalletSection;
    [SerializeField] internal GameObject nonConnectedWalletSection;

    [Header("Thumbnail")]
    [SerializeField] internal RawImage imageAvatarThumbnail;
    [SerializeField] protected internal Button buttonToggleMenu;

    [Header("Texts")]
    [SerializeField] internal TextMeshProUGUI textName;
    [SerializeField] internal TextMeshProUGUI textPostfix;
    [SerializeField] internal TextMeshProUGUI textAddress;

    [Header("Buttons")]
    [SerializeField] protected internal Button buttonLogOut;
    [SerializeField] protected internal Button buttonSignUp;
    [SerializeField] protected internal Button buttonClaimName;
    [SerializeField] protected internal Button buttonCopyAddress;
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
    [SerializeField] internal TMP_InputField descriptionPreviewInput;
    [SerializeField] internal TMP_InputField descriptionEditionInput;
    [SerializeField] internal GameObject charLimitDescriptionContainer;
    [SerializeField] internal TextMeshProUGUI textCharLimitDescription;
    [SerializeField] internal GameObject descriptionContainer;

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

    private InputAction_Trigger.Triggered closeActionDelegate;
    private Coroutine copyToastRoutine = null;
    private UserProfile profile = null;

    private HUDCanvasCameraModeController hudCanvasCameraModeController;


    public BaseComponentView BaseView => this;
    public GameObject GameObject => gameObject;
    public RectTransform ExpandedMenu => mainRootLayout;
    public RectTransform TutorialReference => tutorialTooltipReference;


    public override void RefreshControl() { }

    public bool HasManaCounterView() => manaCounterView != null;

    public bool HasPolygonManaCounterView() => polygonManaCounterView != null;

    public bool IsDesciptionIsLongerThanMaxCharacters() => descriptionEditionInput.characterLimit < descriptionEditionInput.text.Length;

    public void SetManaBalance(string balance) => manaCounterView.SetBalance(balance);

    public void SetPolygonBalance(double balance) => polygonManaCounterView.SetBalance(balance);

    public void SetWalletSectionEnabled(bool isEnabled) => connectedWalletSection.SetActive(isEnabled);

    public void SetNonWalletSectionEnabled(bool isEnabled) => nonConnectedWalletSection.SetActive(isEnabled);

    public void SetStartMenuButtonActive(bool isActive) => isStartMenuInitialized = isActive;

    public void ToggleMenu()
    {
        if (showHideAnimator.isVisible)
            HideMenu();
        else
        {
            showHideAnimator.Show();
            CommonScriptableObjects.isProfileHUDOpen.Set(true);
            Opened?.Invoke(this, EventArgs.Empty);
        }
    }

    public void HideMenu()
    {
        if (showHideAnimator.isVisible)
        {
            showHideAnimator.Hide();
            CommonScriptableObjects.isProfileHUDOpen.Set(false);
            Closed?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SetVisibility(bool visible)
    {
        if (visible && !showHideAnimator.isVisible)
            showHideAnimator.Show();
        else if (!visible && showHideAnimator.isVisible)
            showHideAnimator.Hide();
    }

    public void SetProfile(UserProfile userProfile)
    {
        expandedLayoutAnimator.Show();

        profile = userProfile;
        if (userProfile.hasClaimedName)
            HandleClaimedProfileName(userProfile);
        else
            HandleUnverifiedProfileName(userProfile);

        SetConnectedWalletSectionActive(userProfile.hasConnectedWeb3);
        HandleProfileAddress(userProfile);
        HandleProfileSnapshot(userProfile);
        SetDescription(userProfile.description);
        SetDescriptionEnabled(userProfile.hasConnectedWeb3);
    }


    private void OnEnable() => closeAction.OnTriggered += closeActionDelegate;

    private void OnDisable() => closeAction.OnTriggered -= closeActionDelegate;

    private void Awake()
    {
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

        closeActionDelegate = (x) => HideMenu();

        buttonToggleMenu.onClick.AddListener(OpenStartMenu);
        buttonCopyAddress.onClick.AddListener(CopyAddress);
        buttonEditName.onPointerDown += () => ActivateProfileNameEditionMode(true);
        buttonEditNamePrefix.onPointerDown += () => ActivateProfileNameEditionMode(true);
        inputName.onValueChanged.AddListener(UpdateNameCharLimit);
        inputName.onDeselect.AddListener((x) =>
        {
            ActivateProfileNameEditionMode(false);
            NameSubmitted?.Invoke(this, x);
        });
        inputName.onEndEdit.AddListener(x => NameSubmitted(this, x));

        descriptionPreviewInput.onSelect.AddListener(x =>
        {
            SetDescriptionIsEditing(true);
            UpdateDescriptionCharLimit(descriptionPreviewInput.text);
        });
        descriptionEditionInput.onValueChanged.AddListener(UpdateDescriptionCharLimit);
        descriptionEditionInput.onDeselect.AddListener(x => SetDescriptionIsEditing(false));
        descriptionEditionInput.onEndEdit.AddListener(x => DescriptionSubmitted?.Invoke(this, x));
        copyToast.gameObject.SetActive(false);
        hudCanvasCameraModeController = new HUDCanvasCameraModeController(GetComponent<Canvas>(), DataStore.i.camera.hudsCamera);
    }

    private void UpdateNameCharLimit(string newValue) => textCharLimit.text = $"{newValue.Length}/{inputName.characterLimit}";

    public void SetProfileName(string newName) => textName.text = newName;

    private void UpdateDescriptionCharLimit(string newValue) =>
        textCharLimitDescription.text = $"{newValue.Length}/{descriptionPreviewInput.characterLimit}";

    private void SetDescriptionEnabled(bool enabled) => descriptionContainer.SetActive(enabled);

    private void OpenStartMenu()
    {
        if (isStartMenuInitialized)
        {
            if (!DataStore.i.exploreV2.isOpen.Get())
            {
                var exploreV2Analytics = new ExploreV2Analytics.ExploreV2Analytics();
                exploreV2Analytics.SendStartMenuVisibility(true, ExploreUIVisibilityMethod.FromClick);
            }
            DataStore.i.exploreV2.isOpen.Set(true);
        }
        else
            ToggleMenu();
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
            : userProfile.userName.Substring(0, userProfile.userName.Length - NAME_POSTFIX_LENGTH - 1);

        textPostfix.text = $"#{userProfile.userId.Substring(userProfile.userId.Length - NAME_POSTFIX_LENGTH)}";
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

        copyTooltip.SetActive(false);
        if (copyToastRoutine != null)
            StopCoroutine(copyToastRoutine);

        copyToastRoutine = StartCoroutine(ShowCopyToast());
    }

    private IEnumerator ShowCopyToast()
    {
        if (!copyToast.gameObject.activeSelf)
            copyToast.gameObject.SetActive(true);

        copyToast.Show();
        yield return new WaitForSeconds(COPY_TOAST_VISIBLE_TIME);
        copyToast.Hide();
    }

    internal void ActivateProfileNameEditionMode(bool activate)
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

    public void SetDescriptionIsEditing(bool isEditing)
    {
        charLimitDescriptionContainer.SetActive(isEditing);
        descriptionEditionInput.gameObject.SetActive(isEditing);
        descriptionPreviewInput.gameObject.SetActive(!isEditing);

        if (isEditing)
        {
            descriptionEditionInput.text = descriptionPreviewInput.text;
            StartCoroutine(SelectComponentOnNextFrame(descriptionEditionInput));
        }
    }

    private IEnumerator SelectComponentOnNextFrame(Selectable selectable)
    {
        yield return null;
        selectable.Select();
    }

    public void SetDescription(string description)
    {
        descriptionPreviewInput.text = description;
        descriptionEditionInput.text = description;
    }

    public void SetCardAsFullScreenMenuMode(bool isActive)
    {
        buttonToggleMenu.gameObject.SetActive(!isActive);
        mainCanvas.sortingOrder = isActive ? 4 : 1;
    }
}
