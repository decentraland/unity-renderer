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

    RectTransform Transform { get; }
    bool IsActive { get; }
    int PrivateChannelsCount { get; }
    int PublicChannelsCount { get; }

    void Initialize(IChatController chatController);
    void Show();
    void Hide();
    void SetPrivateChat(PrivateChatModel model);
    void RemovePrivateChat(string userId);
    void SetPublicChat(PublicChatModel model);
    void RemovePublicChat(string channelId);
    void ShowPrivateChatsLoading();
    void HidePrivateChatsLoading();
    void RefreshBlockedDirectMessages(List<string> blockedUsers);
    void Dispose();
    void ClearFilter();
    void HideMoreChatsToLoadHint();
    void ShowMoreChatsToLoadHint(int count);
    void ShowMoreChatsLoading();
    void HideMoreChatsLoading();
    void ShowSearchLoading();
    void HideSearchLoading();
    void Filter(Dictionary<string,PrivateChatModel> privateChats, Dictionary<string,PublicChatModel> publicChannels);
    bool ContainsPrivateChannel(string userId);
}