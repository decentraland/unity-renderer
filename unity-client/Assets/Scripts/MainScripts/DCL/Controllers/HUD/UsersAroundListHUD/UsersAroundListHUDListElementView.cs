using System;
using System.Collections;
using DCL.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

internal class UsersAroundListHUDListElementView : MonoBehaviour, IPoolLifecycleHandler, IPointerEnterHandler, IPointerExitHandler
{
    private static readonly int talkingAnimation = Animator.StringToHash("Talking");

    public event Action<string, bool> OnMuteUser;
    public event Action<Vector3, string> OnShowUserContexMenu;

    [SerializeField] internal TextMeshProUGUI userName;
    [SerializeField] internal RawImage avatarPreview;
    [SerializeField] internal GameObject blockedGO;
    [SerializeField] internal Button soundButton;
    [SerializeField] internal GameObject muteGO;
    [SerializeField] internal GameObject backgroundHover;
    [SerializeField] internal Button menuButton;
    [SerializeField] internal Transform contexMenuRefPosition;
    [SerializeField] internal Animator talkingAnimator;

    private UserProfile profile;
    private bool isMuted = false;
    private bool isRecording = false;

    private void Start()
    {
        soundButton.onClick.AddListener(OnSoundButtonPressed);
        menuButton.onClick.AddListener(() =>
        {
            if (profile)
            {
                OnShowUserContexMenu?.Invoke(contexMenuRefPosition.position, profile.userId);
            }
        });
    }

    private void OnEnable()
    {
        talkingAnimator.SetBool(talkingAnimation, isRecording);
    }

    public void SetUserProfile(UserProfile profile)
    {
        this.profile = profile;

        userName.text = profile.userName;

        if (profile.faceSnapshot)
        {
            SetAvatarPreviewImage(profile.faceSnapshot);
        }
        else
        {
            profile.OnFaceSnapshotReadyEvent += SetAvatarPreviewImage;
        }
    }

    public void SetMuted(bool isMuted)
    {
        this.isMuted = isMuted;
        muteGO.SetActive(isMuted);
    }

    public void SetRecording(bool isRecording)
    {
        this.isRecording = isRecording;
        talkingAnimator.SetBool(talkingAnimation, isRecording);
    }

    public void SetBlocked(bool blocked)
    {
        blockedGO.SetActive(blocked);
    }

    public void OnPoolRelease()
    {
        avatarPreview.texture = null;
        userName.text = string.Empty;
        isMuted = false;
        isRecording = false;

        if (profile)
        {
            profile.OnFaceSnapshotReadyEvent -= SetAvatarPreviewImage;
            profile = null;
        }
        gameObject.SetActive(false);
    }

    public void OnPoolGet()
    {
        muteGO.SetActive(false);
        talkingAnimator.SetBool(talkingAnimation, false);
        avatarPreview.texture = null;
        userName.text = string.Empty;
        backgroundHover.SetActive(false);
        menuButton.gameObject.SetActive(false);
        blockedGO.SetActive(false);
        gameObject.SetActive(true);
    }

    void SetAvatarPreviewImage(Texture texture)
    {
        avatarPreview.texture = texture;
    }

    void OnSoundButtonPressed()
    {
        if (profile == null)
        {
            return;
        }
        OnMuteUser?.Invoke(profile.userId, !isMuted);
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        backgroundHover.SetActive(true);
        menuButton.gameObject.SetActive(true);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        backgroundHover.SetActive(false);
        menuButton.gameObject.SetActive(false);
    }
}
