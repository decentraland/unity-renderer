using DCL;
using UnityEngine;
using UnityEngine.UI;

public class FriendEntry : FriendEntryBase
{
    public event System.Action<FriendEntry> OnWhisperClick;
    public event System.Action<FriendEntry> OnJumpInClick;

    [SerializeField] internal JumpInButton jumpInButton;
    [SerializeField] internal Button whisperButton;
    [SerializeField] internal UnreadNotificationBadge unreadNotificationBadge;

    public override void Awake()
    {
        base.Awake();

        whisperButton.onClick.RemoveAllListeners();
        whisperButton.onClick.AddListener(() => OnWhisperClick?.Invoke(this));
    }

    private void Start()
    {
        unreadNotificationBadge?.Initialize(ChatController.i, userId, Environment.i.serviceLocator.Get<ILastReadMessagesService>());
        jumpInButton.Initialize(FriendsController.i, userId);
        jumpInButton.OnClick += () => OnJumpInClick?.Invoke(this);
    }
}