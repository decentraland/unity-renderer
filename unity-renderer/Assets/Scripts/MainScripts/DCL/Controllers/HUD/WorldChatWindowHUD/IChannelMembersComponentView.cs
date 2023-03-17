using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace DCL.Chat.HUD
{
    public interface IChannelMembersComponentView
    {
        event Action<string> OnSearchUpdated;
        event Action OnRequestMoreMembers;

        Func<string, CancellationToken, UniTask<UserProfile>> LoadFullProfileStrategy { set; }
        int EntryCount { get; }

        void ClearAllEntries();
        void ShowLoading();
        void Dispose();
        void Set(ChannelMemberEntryModel user);
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
