using System;
using System.Collections;
using DCL;
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
    [SerializeField] internal GameObject friendLabel;
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
        if ( profile != null )
            profile.snapshotObserver.AddListener(SetAvatarSnapshotImage);

        if ( talkingAnimator.isActiveAndEnabled )
            talkingAnimator.SetBool(talkingAnimation, isRecording);
    }

    private void OnDisable()
    {
        if ( profile != null )
            profile.snapshotObserver.RemoveListener(SetAvatarSnapshotImage);
    }

    private void OnDestroy()
    {
        if ( profile != null )
            profile.snapshotObserver.RemoveListener(SetAvatarSnapshotImage);
    }

    public void SetUserProfile(UserProfile profile)
    {
        userName.text = profile.userName;

        SetupFriends(profile.userId);

        if (this.profile != null && isActiveAndEnabled)
        {
            this.profile.snapshotObserver.RemoveListener(SetAvatarSnapshotImage);
            profile.snapshotObserver.AddListener(SetAvatarSnapshotImage);
        }

        this.profile = profile;
    }

    public void SetMuted(bool isMuted)
    {
        this.isMuted = isMuted;
        muteGO.SetActive(isMuted);
    }

    public void SetRecording(bool isRecording)
    {
        this.isRecording = isRecording;

        if ( talkingAnimator.isActiveAndEnabled )
            talkingAnimator.SetBool(talkingAnimation, isRecording);
    }

    public void SetBlocked(bool blocked) { blockedGO.SetActive(blocked); }

    public void OnPoolRelease()
    {
        avatarPreview.texture = null;
        userName.text = string.Empty;
        isMuted = false;
        isRecording = false;

        if (profile != null)
        {
            profile.snapshotObserver?.RemoveListener(SetAvatarSnapshotImage);
            profile = null;
        }

        if (FriendsController.i != null)
        {
            FriendsController.i.OnUpdateFriendship -= OnFriendActionUpdate;
        }

        gameObject.SetActive(false);
    }

    public void OnPoolGet()
    {
        muteGO.SetActive(false);

        if (talkingAnimator.isActiveAndEnabled)
            talkingAnimator.SetBool(talkingAnimation, false);

        avatarPreview.texture = null;
        userName.text = string.Empty;
        backgroundHover.SetActive(false);
        menuButton.gameObject.SetActive(false);
        blockedGO.SetActive(false);
        gameObject.SetActive(true);
    }

    void SetupFriends(string userId)
    {
        if (FriendsController.i == null)
        {
            return;
        }

        if (FriendsController.i.friends.TryGetValue(userId, out FriendsController.UserStatus status))
        {
            SetupFriendship(status.friendshipStatus);
        }
        else
        {
            SetupFriendship(FriendshipStatus.NOT_FRIEND);
        }

        FriendsController.i.OnUpdateFriendship -= OnFriendActionUpdate;
        FriendsController.i.OnUpdateFriendship += OnFriendActionUpdate;
    }

    void SetAvatarSnapshotImage(Texture2D texture) { avatarPreview.texture = texture; }

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

    void OnFriendActionUpdate(string userId, FriendshipAction action)
    {
        if (profile.userId != userId)
        {
            return;
        }

        friendLabel.SetActive(action == FriendshipAction.APPROVED);
    }

    void SetupFriendship(FriendshipStatus friendshipStatus) { friendLabel.SetActive(friendshipStatus == FriendshipStatus.FRIEND); }
}