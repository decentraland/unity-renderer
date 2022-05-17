using System;
using SocialFeaturesAnalytics;
using UnityEngine;
using UnityEngine.UI;
using Environment = DCL.Environment;

public class FriendEntry : FriendEntryBase
{
    public event Action<FriendEntry> OnWhisperClick;
    public event Action<FriendEntry> OnJumpInClick;

    [SerializeField] internal JumpInButton jumpInButton;
    [SerializeField] internal Button whisperButton;
    [SerializeField] internal UnreadNotificationBadge unreadNotificationBadge;
    [SerializeField] private Button rowButton;

    public override void Awake()
    {
        base.Awake();

        whisperButton.onClick.RemoveAllListeners();
        whisperButton.onClick.AddListener(() => OnWhisperClick?.Invoke(this));
        rowButton.onClick.RemoveAllListeners();
        rowButton.onClick.AddListener(() => OnWhisperClick?.Invoke(this));
    }

    private void Start()
    {
        unreadNotificationBadge?.Initialize(ChatController.i, model.userId, Environment.i.serviceLocator.Get<ILastReadMessagesService>());
        jumpInButton.Initialize(
            FriendsController.i, model.userId, 
            new SocialAnalytics(
                Environment.i.platform.serviceProviders.analytics,
                new UserProfileWebInterfaceBridge()));
        jumpInButton.OnClick += () => OnJumpInClick?.Invoke(this);
    }
}