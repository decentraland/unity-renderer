using SocialFeaturesAnalytics;
using System;
using UnityEngine;

public interface IPrivateChatComponentView
{
    event Action OnPressBack;
    event Action OnMinimize;
    event Action OnClose;
    event Action<string> OnUnfriend;
    event Action<bool> OnFocused;
    event Action OnScrollUpToTheTop;

    IChatHUDComponentView ChatHUD { get; }
    bool IsActive { get; }
    RectTransform Transform { get; }
    bool IsFocused { get; }

    void Initialize(IFriendsController friendsController, ISocialAnalytics socialAnalytics);
    void Setup(UserProfile profile, bool isOnline, bool isBlocked);
    void Show();
    void Hide();
    void Dispose();
    void ActivatePreview();
    void DeactivatePreview();
    void SetLoadingMessagesActive(bool isActive);
    void SetOldMessagesLoadingActive(bool isActive);
}