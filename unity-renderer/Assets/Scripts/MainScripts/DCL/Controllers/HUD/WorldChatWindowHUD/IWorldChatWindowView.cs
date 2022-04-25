using System;
using UnityEngine;

public interface IWorldChatWindowView
{
    event Action OnClose;
    event Action<string> OnOpenPrivateChat;
    event Action<string> OnOpenPublicChannel;
    
    RectTransform Transform { get; }
    bool IsActive { get; }

    void Initialize(IChatController chatController, ILastReadMessagesService lastReadMessagesService);
    void Show();
    void Hide();
    void SetPrivateChat(PrivateChatModel model);
    void SetPublicChannel(PublicChatChannelModel model);
    void ShowPrivateChatsLoading();
    void HidePrivateChatsLoading();
    void Dispose();
}