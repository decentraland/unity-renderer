using System;
using NSubstitute;
using UnityEngine;

public class PublicChatChannelWindowMock : MonoBehaviour, IChannelChatWindowView
{
    public event Action OnClose;
    public event Action OnBack;
    public event Action OnDeactivatePreview;
    public event Action OnActivatePreview;

    public bool IsActive => gameObject.activeSelf;
    public IChatHUDComponentView ChatHUD => Substitute.For<IChatHUDComponentView>();
    public RectTransform Transform => (RectTransform) transform;
    public string channel => "#mockedChannel";

    private void Awake()
    {
        gameObject.AddComponent<RectTransform>();
    }

    public void Dispose() => Destroy(gameObject);

    public void Hide() => gameObject.SetActive(false);

    public void Show() => gameObject.SetActive(true);

    public void Setup(string channelId, string name, string description)
    {
    }
}