using DCL.Chat.HUD;
using DCL.Social.Chat;
using System;
using UnityEngine;

public interface IPublicChatWindowView
{
    event Action OnClose;
    event Action OnBack;
    event Action<bool> OnMuteChanged;
    event Action OnShowMembersList;
    event Action OnHideMembersList;
    event Action OnGoToCrowd;
    bool IsActive { get; }
    IChatHUDComponentView ChatHUD { get; }
    RectTransform Transform { get; }
    IChannelMembersComponentView ChannelMembersHUD { get; }
    void Dispose();
    void Hide();
    void Show();
    void Configure(PublicChatModel model);
    void UpdateMembersCount(int membersAmount);
}
