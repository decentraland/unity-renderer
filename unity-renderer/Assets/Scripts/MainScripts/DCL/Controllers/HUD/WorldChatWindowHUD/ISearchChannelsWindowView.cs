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
    }
}