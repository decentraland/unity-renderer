using DCL.Social.Chat;
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
        event Action OnShowMembersList;
        event Action OnHideMembersList;
        event Action<bool> OnMuteChanged;

        bool IsActive { get; }
        IChatHUDComponentView ChatHUD { get; }
        IChannelMembersComponentView ChannelMembersHUD { get; }
        RectTransform Transform { get; }
        bool IsFocused { get; }

        void Dispose();
        void Hide();
        void Show();
        void Setup(PublicChatModel model);
        void SetLoadingMessagesActive(bool isActive);
        void SetOldMessagesLoadingActive(bool isActive);
    }
}
