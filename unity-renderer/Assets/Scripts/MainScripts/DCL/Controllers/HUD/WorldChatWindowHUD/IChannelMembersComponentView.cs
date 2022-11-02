using System;
using System.Collections.Generic;

namespace DCL.Chat.HUD
{
    public interface IChannelMembersComponentView
    {
        event Action<string> OnSearchUpdated;
        event Action OnRequestMoreMembers;

        int EntryCount { get; }
        void ClearAllEntries();
        void ShowLoading();
        void Dispose();
        void Set(ChannelMemberEntryModel user);
        void Show(bool instant = false);
        void Hide(bool instant = false);
        void ClearSearchInput();
        void HideLoading();
        void ShowLoadingMore();
        void HideLoadingMore();
        void ShowResultsHeader();
        void HideResultsHeader();
    }
}