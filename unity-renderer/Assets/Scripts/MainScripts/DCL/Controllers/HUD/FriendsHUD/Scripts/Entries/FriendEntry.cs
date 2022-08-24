using System;
using SocialFeaturesAnalytics;
using UnityEngine;
using UnityEngine.UI;

public class FriendEntry : FriendEntryBase
{
    public event Action<FriendEntry> OnWhisperClick;
    public event Action<FriendEntry> OnJumpInClick;

    [SerializeField] internal JumpInButton jumpInButton;
    [SerializeField] internal Button whisperButton;
    [SerializeField] internal UnreadNotificationBadge unreadNotificationBadge;
    [SerializeField] private Button rowButton;

    private IChatController chatController;
    private ILastReadMessagesService lastReadMessagesService;
    private IFriendsController friendsController;
    private ISocialAnalytics socialAnalytics;

    public override void Awake()
    {
        base.Awake();

        whisperButton.onClick.RemoveAllListeners();
        whisperButton.onClick.AddListener(() => OnWhisperClick?.Invoke(this));
        rowButton.onClick.RemoveAllListeners();
        rowButton.onClick.AddListener(() => OnWhisperClick?.Invoke(this));
    }

    public void Initialize(IChatController chatController,
        ILastReadMessagesService lastReadMessagesService,
        IFriendsController friendsController,
        ISocialAnalytics socialAnalytics)
    {
        this.chatController = chatController;
        this.lastReadMessagesService = lastReadMessagesService;
        this.friendsController = friendsController;
        this.socialAnalytics = socialAnalytics;
    }

    private void Start()
    {
        unreadNotificationBadge?.Initialize(chatController, Model.userId, lastReadMessagesService);
        jumpInButton.Initialize(friendsController, Model.userId, socialAnalytics);
        jumpInButton.OnClick += () => OnJumpInClick?.Invoke(this);
    }
}