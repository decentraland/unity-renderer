using DCL;
using System;
using System.Collections.Generic;
using UnityEngine;

public interface IWorldChatWindowView
{
    event Action OnClose;
    event Action<string> OnOpenPrivateChat;
    event Action<string> OnOpenPublicChat;
    event Action<string> OnSearchChatRequested;
    event Action OnRequireMorePrivateChats;
    event Action OnOpenChannelSearch;
    event Action<string> OnLeaveChannel;
    event Action OnCreateChannel;
    event Action OnSignUp;
    event Action OnRequireWalletReadme;

    RectTransform Transform { get; }
    bool IsActive { get; }

    void Initialize(IChatController chatController, DataStore_Mentions mentionsDataStore);
    void Show();
    void Hide();
    void SetPrivateChat(PrivateChatModel model);
    void RemovePrivateChat(string userId);
    void SetPublicChat(PublicChatModel model);
    void RemovePublicChat(string channelId);
    void ShowChannelsLoading();
    void HideChannelsLoading();
    void ShowPrivateChatsLoading();
    void HidePrivateChatsLoading();
    void RefreshBlockedDirectMessages(List<string> blockedUsers);
    void RefreshPrivateChatPresence(string userId, bool isOnline);
    void Dispose();
    void DisableSearchMode();
    void HideMoreChatsToLoadHint();
    void ShowMoreChatsToLoadHint(int count);
    void ShowSearchLoading();
    void HideSearchLoading();
    void EnableSearchMode();
    bool ContainsPrivateChannel(string userId);
    void SetCreateChannelButtonActive(bool isActive);
    void SetSearchAndCreateContainerActive(bool isActive);
    void ShowConnectWallet();
    void HideConnectWallet();
    void SetChannelsPromoteLabelVisible(bool isVisible);
}
