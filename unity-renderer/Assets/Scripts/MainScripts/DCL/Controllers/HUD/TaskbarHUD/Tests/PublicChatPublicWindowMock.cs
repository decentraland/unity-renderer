using System;
using NSubstitute;
using UnityEngine;

public class PublicChatPublicWindowMock : MonoBehaviour, IPublicChatWindowView
{
    public event Action OnClose;
    public event Action OnBack;
    public event Action<bool> OnFocused;
    public event Action OnClickOverWindow;

    public bool IsActive => gameObject.activeSelf;
    public IChatHUDComponentView ChatHUD => Substitute.For<IChatHUDComponentView>();
    public RectTransform Transform => (RectTransform) transform;
    public bool IsFocused => false;
    public bool IsInPreviewMode { get; }

    private void Awake()
    {
        gameObject.AddComponent<RectTransform>();
    }

    public void Dispose()
    {
        if (!this) return;
        if (!gameObject) return;
        Destroy(gameObject);
    }

    public void Hide() => gameObject.SetActive(false);

    public void Show() => gameObject.SetActive(true);
    
    public void Configure(PublicChatModel model)
    {
    }

    public void ActivatePreview()
    {
    }

    public void ActivatePreviewInstantly()
    {
    }

    public void DeactivatePreview()
    {
    }
}