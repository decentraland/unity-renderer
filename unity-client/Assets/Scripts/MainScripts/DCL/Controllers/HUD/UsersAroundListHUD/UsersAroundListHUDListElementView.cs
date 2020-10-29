using System;
using System.Collections;
using DCL.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal class UsersAroundListHUDListElementView : MonoBehaviour, IPoolLifecycleHandler
{
    const float USER_NOT_RECORDING_THROTTLING = 2;

    public event Action<string, bool> OnMuteUser;

    [SerializeField] internal TextMeshProUGUI userName;
    [SerializeField] internal RawImage avatarPreview;
    [SerializeField] internal Button soundButton;
    [SerializeField] internal GameObject muteGO;
    [SerializeField] internal GameObject recordingGO;

    private UserProfile profile;
    private bool isMuted = false;
    private bool isRecording = false;
    private Coroutine setUserRecordingRoutine = null;

    private void Start()
    {
        soundButton.onClick.AddListener(OnSoundButtonPressed);
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
}
