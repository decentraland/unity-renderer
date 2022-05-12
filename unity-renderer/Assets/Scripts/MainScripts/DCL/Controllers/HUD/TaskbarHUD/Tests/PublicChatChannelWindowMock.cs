using System;
using NSubstitute;
using UnityEngine;

public class PublicChatChannelWindowMock : MonoBehaviour, IChannelChatWindowView
{
    public event Action OnClose;
    public event Action OnBack;
    public event Action<bool> OnFocused;

    public bool IsActive => gameObject.activeSelf;
    public IChatHUDComponentView ChatHUD => Substitute.For<IChatHUDComponentView>();
    public RectTransform Transform => (RectTransform) transform;
    public bool IsFocused => false;

    private void Awake()
    {
        gameObject.AddComponent<RectTransform>();
    }

    public void Dispose() => Destroy(gameObject);

    public void Hide() => gameObject.SetActive(false);

    public void Show() => gameObject.SetActive(true);
    
    public void Configure(PublicChatChannelModel model)
    {
    }

    public void ActivatePreview()
    {
    }

    public void DeactivatePreview()
    {
    }
}