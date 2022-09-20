using System;
using NSubstitute;
using SocialFeaturesAnalytics;
using UnityEngine;

public class PrivateChatWindowMock : MonoBehaviour, IPrivateChatComponentView
{
    public event Action OnPressBack;
    public event Action OnMinimize;
    public event Action OnClose;
    public event Action<string> OnUnfriend;
    public event Action<bool> OnFocused;
    public event Action OnClickOverWindow;
    public event Action OnRequireMoreMessages;

    public IChatHUDComponentView ChatHUD => Substitute.For<IChatHUDComponentView>();
    public bool IsActive => gameObject.activeSelf;
    public RectTransform Transform => (RectTransform) transform;
    public bool IsFocused => false;
    public bool IsInPreviewMode { get; }
    public bool IsInputFieldSelected => false;

    private bool isDestroyed;

    private void Awake()
    {
        gameObject.AddComponent<RectTransform>();
    }

    private void OnDestroy()
    {
        isDestroyed = true;
    }

    public void Initialize(IFriendsController friendsController, ISocialAnalytics socialAnalytics)
    {
    }

    public void Setup(UserProfile profile, bool isOnline, bool isBlocked)
    {
    }

    public void Show() => gameObject.SetActive(true);

    public void Hide() => gameObject.SetActive(false);

    public void Dispose()
    {
        if (!this) return;
        if (!gameObject) return;
        Destroy(gameObject);
    }

    public void ActivatePreview()
    {
    }

    public void DeactivatePreview()
    {
    }

    public void SetLoadingMessagesActive(bool isActive)
    {
    }

    public void SetOldMessagesLoadingActive(bool isActive)
    {
    }
}