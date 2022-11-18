using System;
using DCL.Chat.Channels;
using UnityEngine;

namespace DCL.Chat.HUD
{
    public interface ISearchChannelsWindowView
    {
        event Action OnBack;
        event Action OnClose;
        event Action<string> OnSearchUpdated;
        event Action OnRequestMoreChannels;
        event Action<string> OnJoinChannel;
        event Action<string> OnLeaveChannel;
        event Action OnCreateChannel;
        event Action<string> OnOpenChannel;
        RectTransform Transform { get; }
        int EntryCount { get; }
        bool IsActive { get; }
        void ClearAllEntries();
        void ShowLoading();
        void Dispose();
        void Set(Channel channel);
        void Show();
        void Hide();
        void ClearSearchInput();
        void HideLoading();
        void ShowLoadingMore();
        void HideLoadingMore();
        void ShowResultsHeader();
        void HideResultsHeader();
        void ShowCreateChannelOnSearch();
        void HideCreateChannelOnSearch();
        void SetCreateChannelButtonsActive(bool isActive);
    }
}