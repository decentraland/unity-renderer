using DCL;
using ExploreV2Analytics;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Environment = DCL.Environment;

public class ProfileHUDView : MonoBehaviour
{
    private const int ADDRESS_CHUNK_LENGTH = 6;
    private const int NAME_POSTFIX_LENGTH = 4;
    private const float COPY_TOAST_VISIBLE_TIME = 3;

    [SerializeField]
    internal ShowHideAnimator mainShowHideAnimator;

    [SerializeField]
    internal ShowHideAnimator menuShowHideAnimator;

    [SerializeField]
    private RectTransform mainRootLayout;

    [SerializeField]
    internal GameObject loadingSpinner;

    [SerializeField]
    internal ShowHideAnimator copyToast;

    [SerializeField]
    internal GameObject copyTooltip;

    [SerializeField]
    internal InputAction_Trigger closeAction;

    [SerializeField]
    internal Canvas mainCanvas;

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
    [SerializeField]
    internal TextMeshProUGUI textName;

    [SerializeField]
    internal TextMeshProUGUI textPostfix;

    [SerializeField]
    internal TextMeshProUGUI textAddress;

    [Header("Buttons")]
    [SerializeField]
    protected internal Button buttonClaimName;

    [SerializeField]
    protected internal Button buttonCopyAddress;

    [SerializeField]
    protected internal Button buttonLogOut;

    [SerializeField]
    protected internal Button buttonSignUp;

    [SerializeField]
    protected internal Button_OnPointerDown buttonTermsOfServiceForConnectedWallets;

    [SerializeField]
    protected internal Button_OnPointerDown buttonPrivacyPolicyForConnectedWallets;

    [SerializeField]
    protected internal Button_OnPointerDown buttonTermsOfServiceForNonConnectedWallets;

    [SerializeField]
    protected internal Button_OnPointerDown buttonPrivacyPolicyForNonConnectedWallets;

    [Header("Name Edition")]
    [SerializeField]
    protected internal Button_OnPointerDown buttonEditName;

    [SerializeField]
    protected internal Button_OnPointerDown buttonEditNamePrefix;

    [SerializeField]
    internal TMP_InputField inputName;

    [SerializeField]
    internal TextMeshProUGUI textCharLimit;

    [SerializeField]
    internal ManaCounterView manaCounterView;

    [SerializeField]
    internal ManaCounterView polygonManaCounterView;

    [Header("Tutorial Config")]
    [SerializeField]
    internal RectTransform tutorialTooltipReference;

    [Header("Description")]
    [SerializeField]
    internal TMP_InputField descriptionPreviewInput;

    [SerializeField]
    internal TMP_InputField descriptionEditionInput;

    [SerializeField]
    internal GameObject charLimitDescriptionContainer;

    [SerializeField]
    internal TextMeshProUGUI textCharLimitDescription;

    [SerializeField]
    internal GameObject descriptionContainer;

    public RectTransform expandedMenu => mainRootLayout;

    private InputAction_Trigger.Triggered closeActionDelegate;

    private Coroutine copyToastRoutine = null;
    private UserProfile profile = null;
    private Regex nameRegex = null;

    internal event Action OnOpen;
    internal event Action OnClose;
    internal bool isStartMenuInitialized = false;

    private void Awake()
    {
        closeActionDelegate = (x) => HideMenu();

        buttonToggleMenu.onClick.AddListener(OpenStartMenu);
        buttonCopyAddress.onClick.AddListener(CopyAddress);
        buttonEditName.onPointerDown += () => ActivateProfileNameEditionMode(true);
        buttonEditNamePrefix.onPointerDown += () => ActivateProfileNameEditionMode(true);
        inputName.onValueChanged.AddListener(UpdateNameCharLimit);
        inputName.onDeselect.AddListener((x) => ActivateProfileNameEditionMode(false));
        descriptionPreviewInput.onSelect.AddListener(x =>
        {
            ActivateDescriptionEditionMode(true);
            UpdateDescriptionCharLimit(descriptionPreviewInput.text);
        });
        descriptionEditionInput.onValueChanged.AddListener(UpdateDescriptionCharLimit);
        descriptionEditionInput.onDeselect.AddListener(x => ActivateDescriptionEditionMode(false));
        copyToast.gameObject.SetActive(false);
    }

    internal void SetProfile(UserProfile userProfile)
    {
        profile = userProfile;
        if (userProfile.hasClaimedName)
        {
            HandleClaimedProfileName(userProfile);
        }
        else
        {
            HandleUnverifiedProfileName(userProfile);
        }

        SetConnectedWalletSectionActive(userProfile.hasConnectedWeb3);
        HandleProfileAddress(userProfile);
        HandleProfileSnapshot(userProfile);
        SetDescription(userProfile.description);
        SetDescriptionEnabled(userProfile.hasConnectedWeb3);
        ForceLayoutToRefreshSize();
    }

    internal void OpenStartMenu()
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
        else
        {
            ToggleMenu();
        }
    }

    public void SetStartMenuButtonActive(bool isActive) { isStartMenuInitialized = isActive; }

    internal void ToggleMenu()
    {
        if (menuShowHideAnimator.isVisible)
        {
            HideMenu();
        }
        else
        {
            menuShowHideAnimator.Show();
            CommonScriptableObjects.isProfileHUDOpen.Set(true);
            OnOpen?.Invoke();
        }
    }

    internal void HideMenu()
    {
        if (menuShowHideAnimator.isVisible)
        {
            menuShowHideAnimator.Hide();
            CommonScriptableObjects.isProfileHUDOpen.Set(false);
            OnClose?.Invoke();
        }
    }

    internal void SetVisibility(bool visible)
    {
        if (visible && !mainShowHideAnimator.isVisible)
            mainShowHideAnimator.Show();
        else if (!visible && mainShowHideAnimator.isVisible)
            mainShowHideAnimator.Hide();
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
        if (!String.IsNullOrEmpty(userProfile.userName) &&
            userProfile.userName.Length > NAME_POSTFIX_LENGTH)
        {
            textName.text = userProfile.userName.Substring(0, userProfile.userName.Length - NAME_POSTFIX_LENGTH - 1);
        }
        else
        {
            textName.text = userProfile.userName;
        }

        textPostfix.text = $"#{userProfile.userId.Substring(userProfile.userId.Length - NAME_POSTFIX_LENGTH)}";
        SetActiveUnverifiedNameGOs(true);
    }

    private void SetConnectedWalletSectionActive(bool active)
    {
        connectedWalletSection.SetActive(active);
        nonConnectedWalletSection.SetActive(!active);
        buttonLogOut.gameObject.SetActive(active);
    }

    private void ForceLayoutToRefreshSize() { LayoutRebuilder.ForceRebuildLayoutImmediate(mainRootLayout); }

    private void SetActiveUnverifiedNameGOs(bool active)
    {
        for (int i = 0; i < hideOnNameClaimed.Length; i++)
        {
            hideOnNameClaimed[i].SetActive(active);
        }
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
        if (profile)
            profile.snapshotObserver.RemoveListener(SetProfileImage);
    }

    private void CopyAddress()
    {
        if (!profile)
        {
            return;
        }

        Environment.i.platform.clipboard.WriteText(profile.userId);

        copyTooltip.gameObject.SetActive(false);
        if (copyToastRoutine != null)
        {
            StopCoroutine(copyToastRoutine);
        }

        copyToastRoutine = StartCoroutine(ShowCopyToast());
    }

    private IEnumerator ShowCopyToast()
    {
        if (!copyToast.gameObject.activeSelf)
        {
            copyToast.gameObject.SetActive(true);
        }

        copyToast.Show();
        yield return new WaitForSeconds(COPY_TOAST_VISIBLE_TIME);
        copyToast.Hide();
    }

    private void OnEnable() { closeAction.OnTriggered += closeActionDelegate; }

    private void OnDisable() { closeAction.OnTriggered -= closeActionDelegate; }

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

    private void UpdateNameCharLimit(string newValue) { textCharLimit.text = $"{newValue.Length}/{inputName.characterLimit}"; }

    internal void SetProfileName(string newName) { textName.text = newName; }

    internal void SetNameRegex(string namePattern) { nameRegex = new Regex(namePattern); }

    internal bool IsValidAvatarName(string name)
    {
        if (nameRegex == null)
            return true;

        return nameRegex.IsMatch(name);
    }

    internal void ActivateDescriptionEditionMode(bool active)
    {
        charLimitDescriptionContainer.SetActive(active);
        descriptionEditionInput.gameObject.SetActive(active);
        descriptionPreviewInput.gameObject.SetActive(!active);

        if (active)
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

    internal void SetDescription(string description)
    {
        descriptionPreviewInput.text = description;
        descriptionEditionInput.text = description;
    }

    private void UpdateDescriptionCharLimit(string newValue) { textCharLimitDescription.text = $"{newValue.Length}/{descriptionPreviewInput.characterLimit}"; }

    private void SetDescriptionEnabled(bool enabled) { descriptionContainer.SetActive(enabled); }

    public void SetCardAsFullScreenMenuMode(bool isActive)
    {
        buttonToggleMenu.gameObject.SetActive(!isActive);
        mainCanvas.sortingOrder = isActive ? 4 : 1;
    }
}