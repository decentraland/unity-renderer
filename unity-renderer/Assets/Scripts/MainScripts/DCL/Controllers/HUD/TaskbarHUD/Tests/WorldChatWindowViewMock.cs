using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldChatWindowViewMock : MonoBehaviour, IWorldChatWindowView
{
    public event Action OnClose;
    public event Action<string> OnOpenPrivateChat;
    public event Action<string> OnOpenPublicChannel;
    public event Action<string> OnUnfriend;
    public event Action<string> OnSearchChannelRequested;
    public event Action OnRequireMorePrivateChats;
    public RectTransform Transform => (RectTransform) transform;
    public bool IsActive => gameObject.activeSelf;
    public int PrivateChannelsCount { get; }

    private bool isDestroyed;

    private void Awake()
    {
        gameObject.AddComponent<RectTransform>();
    }

    private void OnDestroy()
    {
        isDestroyed = true;
    }

    public void Initialize(IChatController chatController, ILastReadMessagesService lastReadMessagesService)
    {
    }

    public void Show() => gameObject.SetActive(true);

    public void Hide() => gameObject.SetActive(false);

    public void SetPrivateChat(PrivateChatModel model)
    {
    }

    public void RemovePrivateChat(string userId)
    {
    }

    public void SetPublicChannel(PublicChatChannelModel model)
    {
    }

    public void ShowPrivateChatsLoading()
    {
    }

    public void HidePrivateChatsLoading()
    {
    }

    public void RefreshBlockedDirectMessages(List<string> blockedUsers)
    {
    }

    public void Dispose()
    {
        if (isDestroyed) return;
        Destroy(gameObject);
    }

    public void ClearFilter()
    {
    }

    public void HideMoreChatsToLoadHint()
    {
    }

    public void ShowMoreChatsToLoadHint()
    {
    }

    public void ShowMoreChatsLoading()
    {
    }

    public void HideMoreChatsLoading()
    {
    }

    public void Filter(Dictionary<string, PrivateChatModel> privateChats, Dictionary<string, PublicChatChannelModel> publicChannels)
    {
    }

    public bool ContainsPrivateChannel(string userId) => false;
}