using System;
using DCL.Chat.Channels;
using DCL.Chat.HUD;
using UnityEngine;

public class SearchChannelsWindowMock : MonoBehaviour, ISearchChannelsWindowView
{
    public event Action OnBack;
    public event Action OnClose;
    public event Action<string> OnSearchUpdated;
    public event Action OnRequestMoreChannels;
    public event Action<string> OnJoinChannel;
    public event Action<string> OnLeaveChannel;
    public event Action OnCreateChannel;
    public RectTransform Transform => (RectTransform) transform;
    public int EntryCount { get; }
    public bool IsActive { get; }

    private void Awake()
    {
        gameObject.AddComponent<RectTransform>();
    }

    public void ClearAllEntries()
    {
    }

    public void ShowLoading()
    {
    }

    public void Dispose()
    {
    }

    public void Set(Channel channel)
    {
    }

    public void Show()
    {
    }

    public void Hide()
    {
    }

    public void ClearSearchInput()
    {
    }

    public void HideLoading()
    {
    }

    public void ShowLoadingMore()
    {
    }

    public void HideLoadingMore()
    {
    }

    public void ShowResultsHeader()
    {
    }

    public void HideResultsHeader()
    {
    }

    public void ShowCreateChannelOnSearch()
    {
    }

    public void HideCreateChannelOnSearch()
    {
    }

    public void SetCreateChannelButtonsActive(bool isActive)
    {
    }
}