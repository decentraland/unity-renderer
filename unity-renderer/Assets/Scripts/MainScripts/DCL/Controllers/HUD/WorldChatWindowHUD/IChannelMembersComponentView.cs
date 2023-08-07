using DCL.Social.Chat;
using System;
using System.Collections.Generic;

namespace DCL.Social.Chat
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
        void Remove(string userId);
        void Show(bool instant = false);
        void Hide(bool instant = false);
        void ClearSearchInput(bool notify = true);
        void HideLoading();
        void ShowLoadingMore();
        void HideLoadingMore();
        void ShowResultsHeader();
        void HideResultsHeader();
    }
}
