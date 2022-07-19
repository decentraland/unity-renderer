using System;
using UnityEngine;

namespace DCL.Chat.HUD
{
    public interface IChatChannelWindowView
    {
        event Action OnClose;
        event Action<bool> OnFocused;
        event Action OnBack;
        event Action OnRequireMoreMessages;
        event Action OnLeaveChannel;

        bool IsActive { get; }
        IChatHUDComponentView ChatHUD { get; }
        RectTransform Transform { get; }
        bool IsFocused { get; }

        void Dispose();
        void Hide();
        void Show();
        void Setup(PublicChatModel model);
        void ActivatePreview();
        void DeactivatePreview();
        void SetLoadingMessagesActive(bool isActive);
        void SetOldMessagesLoadingActive(bool isActive);
    }
}