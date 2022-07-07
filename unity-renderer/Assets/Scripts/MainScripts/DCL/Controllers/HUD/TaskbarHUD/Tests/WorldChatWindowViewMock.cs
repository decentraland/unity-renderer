using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldChatWindowViewMock : MonoBehaviour, IWorldChatWindowView
{
    public event Action OnClose;
    public event Action<string> OnOpenPrivateChat;
    public event Action<string> OnOpenPublicChat;
    public event Action<string> OnUnfriend;
    public event Action<string> OnSearchChannelRequested;
    public event Action OnRequireMorePrivateChats;

    public RectTransform Transform => (RectTransform) transform;
    public bool IsActive => gameObject.activeSelf;
    public int PrivateChannelsCount { get; }
    public int PublicChannelsCount { get; }

    private bool isDestroyed;

    private void Awake()
    {
        gameObject.AddComponent<RectTransform>();
    }

    private void OnDestroy()
    {
        isDestroyed = true;
    }

    public void Initialize(IChatController chatController)
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

    public void SetPublicChat(PublicChatModel model)
    {
    }

    public void RemovePublicChat(string channelId)
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

    public void ShowMoreChatsToLoadHint(int count)
    {
    }

    public void ShowMoreChatsLoading()
    {
    }

    public void HideMoreChatsLoading()
    {
    }

    public void ShowSearchLoading()
    {
    }

    public void HideSearchLoading()
    {
    }

    public void Filter(Dictionary<string, PrivateChatModel> privateChats, Dictionary<string, PublicChatModel> publicChannels)
    {
    }

    public bool ContainsPrivateChannel(string userId) => false;
}