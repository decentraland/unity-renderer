using System;
using NSubstitute;
using UnityEngine;

public class PrivateChatWindowMock : MonoBehaviour, IPrivateChatComponentView
{
    public event Action OnPressBack;
    public event Action OnMinimize;
    public event Action OnClose;
    public event Action<string> OnUnfriend;

    public IChatHUDComponentView ChatHUD => Substitute.For<IChatHUDComponentView>();
    public bool IsActive => gameObject.activeSelf;
    public RectTransform Transform => (RectTransform) transform;

    private void Awake()
    {
        gameObject.AddComponent<RectTransform>();
    }

    public void Setup(UserProfile profile, bool isOnline, bool isBlocked)
    {
    }

    public void Show() => gameObject.SetActive(true);

    public void Hide() => gameObject.SetActive(false);

    public void Dispose() => Destroy(gameObject);
}