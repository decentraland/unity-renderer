using System;
using UnityEngine;

namespace DCL.Chat.HUD
{
    // TODO: rename this class since it conflicts with IChannelChatWindowView which should also be renamed
    public interface IChatChannelWindowViewRenamePlz
    {
        event Action OnClose;
        event Action<bool> OnFocused;
        event Action OnBack;
        event Action OnRequireMoreMessages;

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