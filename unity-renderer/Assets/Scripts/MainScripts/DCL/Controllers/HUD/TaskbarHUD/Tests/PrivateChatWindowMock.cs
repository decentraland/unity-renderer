using System;
using NSubstitute;
using UnityEngine;

public class PrivateChatWindowMock : MonoBehaviour, IPrivateChatComponentView
{
    public event Action OnPressBack;
    public event Action OnMinimize;
    public event Action OnClose;
    public event Action<string> OnUnfriend;
    public event Action<bool> OnFocused;

    public IChatHUDComponentView ChatHUD => Substitute.For<IChatHUDComponentView>();
    public bool IsActive => gameObject.activeSelf;
    public RectTransform Transform => (RectTransform) transform;
    public bool IsFocused => false;

    private bool isDestroyed;

    private void Awake()
    {
        gameObject.AddComponent<RectTransform>();
    }

    private void OnDestroy()
    {
        isDestroyed = true;
    }

    public void Setup(UserProfile profile, bool isOnline, bool isBlocked)
    {
    }

    public void Show() => gameObject.SetActive(true);

    public void Hide() => gameObject.SetActive(false);

    public void Dispose()
    {
        if (isDestroyed) return;
        Destroy(gameObject);
    }

    public void ActivatePreview()
    {
    }

    public void DeactivatePreview()
    {
    }
}