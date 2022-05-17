using System;
using System.Collections.Generic;
using UnityEngine;

public interface IWorldChatWindowView
{
    event Action OnClose;
    event Action<string> OnOpenPrivateChat;
    event Action<string> OnOpenPublicChannel;
    event Action<string> OnUnfriend;
    
    RectTransform Transform { get; }
    bool IsActive { get; }

    void Initialize(IChatController chatController, ILastReadMessagesService lastReadMessagesService);
    void Show();
    void Hide();
    void SetPrivateChat(PrivateChatModel model);
    void RemovePrivateChat(string userId);
    void SetPublicChannel(PublicChatChannelModel model);
    void ShowPrivateChatsLoading();
    void HidePrivateChatsLoading();
    void RefreshBlockedDirectMessages(List<string> blockedUsers);
    void Dispose();
}