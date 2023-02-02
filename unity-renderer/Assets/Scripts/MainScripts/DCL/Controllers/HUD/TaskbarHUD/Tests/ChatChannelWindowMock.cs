using DCL.Chat.HUD;
using NSubstitute;
using System;
using UnityEngine;

public class ChatChannelWindowMock : MonoBehaviour, IChatChannelWindowView
{
    public event Action OnClose;
    public event Action<bool> OnFocused;
    public event Action OnBack;
    public event Action OnRequireMoreMessages;
    public event Action OnLeaveChannel;
    public event Action OnShowMembersList;
    public event Action OnHideMembersList;
    public event Action<bool> OnMuteChanged;
    public bool IsActive { get; }
    public IChatHUDComponentView ChatHUD => Substitute.For<IChatHUDComponentView>();
    public IChannelMembersComponentView ChannelMembersHUD { get; }
    public RectTransform Transform => (RectTransform) transform;
    public bool IsFocused { get; }

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

    public void Hide() =>
        gameObject.SetActive(false);

    public void Show() =>
        gameObject.SetActive(true);

    public void Setup(PublicChatModel model)
    {
    }

    public void SetLoadingMessagesActive(bool isActive)
    {
    }

    public void SetOldMessagesLoadingActive(bool isActive)
    {
    }
}
