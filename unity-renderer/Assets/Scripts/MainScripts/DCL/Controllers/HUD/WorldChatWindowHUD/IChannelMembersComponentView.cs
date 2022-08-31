using System;
using UnityEngine;

namespace DCL.Chat.HUD
{
    public interface IChannelMembersComponentView
    {
        event Action<string> OnSearchUpdated;
        event Action OnRequestMoreMembers;

        int EntryCount { get; }
        bool IsActive { get; }
        void ClearAllEntries();
        void ShowLoading();
        void Dispose();
        void Set(ChannelMemberEntryModel member);
        void Show(bool instant = false);
        void Hide(bool instant = false);
        void ClearSearchInput();
        void HideLoading();
        void ShowLoadingMore();
        void HideLoadingMore();
    }
}