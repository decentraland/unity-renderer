using System;
using UnityEngine;

namespace DCL.Social.Chat
{
    public interface IChatChannelWindowView
    {
        event Action OnClose;
        event Action OnBack;
        event Action OnRequireMoreMessages;
        event Action OnLeaveChannel;
        event Action OnShowMembersList;
        event Action OnHideMembersList;
        event Action<bool> OnMuteChanged;
        event Action<string> OnCopyNameRequested;

        bool IsActive { get; }
        IChatHUDComponentView ChatHUD { get; }
        IChannelMembersComponentView ChannelMembersHUD { get; }
        RectTransform Transform { get; }

        void Dispose();
        void Hide();
        void Show();
        void Setup(PublicChatModel model);
        void SetLoadingMessagesActive(bool isActive);
        void SetOldMessagesLoadingActive(bool isActive);
    }
}
