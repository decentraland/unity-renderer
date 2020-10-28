using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal class ProfileHUDView : MonoBehaviour
{
    private const int ADDRESS_CHUNK_LENGTH = 6;
    private const int NAME_POSTFIX_LENGTH = 4;
    private const float COPY_TOAST_VISIBLE_TIME = 3;

    [SerializeField] internal ShowHideAnimator mainShowHideAnimator;
    [SerializeField] internal ShowHideAnimator menuShowHideAnimator;
    [SerializeField] internal GameObject loadingSpinner;

    [SerializeField] internal ShowHideAnimator copyToast;
    [SerializeField] internal GameObject copyTooltip;
    [SerializeField] internal InputAction_Trigger closeAction;

    [Header("Hide GOs on claimed name")]
    [SerializeField] internal GameObject[] hideOnNameClaimed;

    [Header("Thumbnail")]
    [SerializeField] internal RawImage imageAvatarThumbnail;
    [SerializeField] internal Button buttonToggleMenu;

    [Header("Texts")]
    [SerializeField] internal TextMeshProUGUI textName;
    [SerializeField] internal TextMeshProUGUI textPostfix;
    [SerializeField] internal TextMeshProUGUI textAddress;

    [Header("Buttons")]
    [SerializeField] internal Button buttonEditUnverifiedName;
    [SerializeField] internal Button buttonClaimName;
    [SerializeField] internal Button buttonCopyAddress;
    [SerializeField] internal Button buttonLogOut;

    private InputAction_Trigger.Triggered closeActionDelegate;

    private Coroutine copyToastRoutine = null;
    private UserProfile profile = null;

    private void Awake()
    {
        closeActionDelegate = (x) => HideMenu();

        buttonToggleMenu.onClick.AddListener(ToggleMenu);
        buttonCopyAddress.onClick.AddListener(CopyAddress);
        copyToast.gameObject.SetActive(false);
    }

    public void SetProfile(UserProfile userProfile)
    {
        if (userProfile.hasClaimedName)
        {
            HandleClaimedProfileName(userProfile);
        }
        else
        {
            HandleUnverifiedProfileName(userProfile);
        }

        HandleProfileAddress(userProfile);
        HandleProfileSnapshot(userProfile);
        profile = userProfile;
    }

    public void ToggleMenu()
    {
        if (menuShowHideAnimator.isVisible)
        {
            HideMenu();
        }
        else
        {
            menuShowHideAnimator.Show();
            CommonScriptableObjects.isProfileHUDOpen.Set(true);
        }
    }

    public void HideMenu()
    {
        if (menuShowHideAnimator.isVisible)
        {
            menuShowHideAnimator.Hide();
            CommonScriptableObjects.isProfileHUDOpen.Set(false);
        }
    }

    public void SetVisibility(bool visible)
    {
        if (visible && !mainShowHideAnimator.isVisible)
            mainShowHideAnimator.Show();
        else if (!visible && mainShowHideAnimator.isVisible)
            mainShowHideAnimator.Hide();
    }

    private void HandleProfileSnapshot(UserProfile userProfile)
    {
        if (profile)
        {
            profile.OnFaceSnapshotReadyEvent -= SetProfileImage;
        }

        if (userProfile.faceSnapshot != null)
        {
            SetProfileImage(userProfile.faceSnapshot);
        }
        else
        {
            loadingSpinner.SetActive(true);
            userProfile.OnFaceSnapshotReadyEvent += SetProfileImage;
        }
    }

    private void HandleClaimedProfileName(UserProfile userProfile)
    {
        textName.text = userProfile.userName;
        SetActiveUnverifiedNameGOs(false);
    }

    private void HandleUnverifiedProfileName(UserProfile userProfile)
    {
        textName.text = userProfile.userName;
        textPostfix.text = $".{userProfile.userId.Substring(userProfile.userId.Length - NAME_POSTFIX_LENGTH)}";
        SetActiveUnverifiedNameGOs(true);
    }

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
        profile.OnFaceSnapshotReadyEvent -= SetProfileImage;
        imageAvatarThumbnail.texture = texture;
        loadingSpinner.SetActive(false);
    }

    private void OnDestroy()
    {
        if (profile)
        {
            profile.OnFaceSnapshotReadyEvent -= SetProfileImage;
        }
    }

    private void CopyAddress()
    {
        if (!profile)
        {
            return;
        }

        DCL.Environment.i.clipboard.WriteText(profile.userId);

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

    private void OnEnable()
    {
        closeAction.OnTriggered += closeActionDelegate;
    }

    private void OnDisable()
    {
        closeAction.OnTriggered -= closeActionDelegate;
    }
}
