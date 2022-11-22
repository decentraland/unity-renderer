using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldChatWindowViewMock : MonoBehaviour, IWorldChatWindowView
{
    public event Action OnClose;
    public event Action<string> OnOpenPrivateChat;
    public event Action<string> OnOpenPublicChat;
    public event Action<string> OnSearchChatRequested;
    public event Action OnRequireMorePrivateChats;
    public event Action OnOpenChannelSearch;
    public event Action<string> OnLeaveChannel;
    public event Action OnCreateChannel;
    public event Action OnSignUp;
    public event Action OnRequireWalletReadme;

    public RectTransform Transform => (RectTransform) transform;
    public bool IsActive => gameObject.activeSelf;

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

    public void DisableSearchMode()
    {
    }

    public void HideMoreChatsToLoadHint()
    {
    }

    public void ShowMoreChatsToLoadHint(int count)
    {
    }

    public void ShowChannelsLoading()
    {
    }

    public void HideChannelsLoading()
    {
    }

    public void ShowSearchLoading()
    {
    }

    public void HideSearchLoading()
    {
    }

    public void EnableSearchMode()
    {
    }

    public bool ContainsPrivateChannel(string userId) => false;

    public void SetCreateChannelButtonActive(bool isActive)
    {
    }

    public void SetSearchAndCreateContainerActive(bool isActive)
    {
    }

    public void ShowConnectWallet()
    {
    }

    public void HideConnectWallet()
    {
    }

    public void SetChannelsPromoteLabelVisible(bool isVisible)
    {
    }
}