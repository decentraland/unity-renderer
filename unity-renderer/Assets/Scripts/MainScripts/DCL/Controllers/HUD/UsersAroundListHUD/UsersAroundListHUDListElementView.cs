using System;
using System.Collections;
using DCL.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

internal class UsersAroundListHUDListElementView : MonoBehaviour, IPoolLifecycleHandler, IPointerEnterHandler, IPointerExitHandler
{
    const float USER_NOT_RECORDING_THROTTLING = 2;

    public event Action<string, bool> OnMuteUser;
    public event Action<Vector3, string> OnShowUserContexMenu;

    [SerializeField] internal TextMeshProUGUI userName;
    [SerializeField] internal RawImage avatarPreview;
    [SerializeField] internal Button soundButton;
    [SerializeField] internal GameObject muteGO;
    [SerializeField] internal GameObject recordingGO;
    [SerializeField] internal GameObject backgroundHover;
    [SerializeField] internal Button menuButton;
    [SerializeField] internal Transform contexMenuRefPosition;

    private UserProfile profile;
    private bool isMuted = false;
    private bool isRecording = false;
    private Coroutine setUserRecordingRoutine = null;

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
        if (this.isRecording == isRecording)
            return;

        this.isRecording = isRecording;
        if (setUserRecordingRoutine != null)
        {
            StopCoroutine(setUserRecordingRoutine);
        }
        setUserRecordingRoutine = StartCoroutine(SetRecordingRoutine(isRecording));
    }

    public void OnPoolRelease()
    {
        avatarPreview.texture = null;
        userName.text = string.Empty;
        isMuted = false;

        if (profile)
        {
            profile.OnFaceSnapshotReadyEvent -= SetAvatarPreviewImage;
            profile = null;
        }
        if (setUserRecordingRoutine != null)
        {
            StopCoroutine(setUserRecordingRoutine);
            setUserRecordingRoutine = null;
        }
        gameObject.SetActive(false);
    }

    public void OnPoolGet()
    {
        muteGO.SetActive(false);
        recordingGO.SetActive(false);
        avatarPreview.texture = null;
        userName.text = string.Empty;
        backgroundHover.SetActive(false);
        menuButton.gameObject.SetActive(false);
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

    IEnumerator SetRecordingRoutine(bool isRecording)
    {
        if (isRecording)
        {
            recordingGO.SetActive(true);
            yield break;
        }
        yield return WaitForSecondsCache.Get(USER_NOT_RECORDING_THROTTLING);
        recordingGO.SetActive(false);
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
