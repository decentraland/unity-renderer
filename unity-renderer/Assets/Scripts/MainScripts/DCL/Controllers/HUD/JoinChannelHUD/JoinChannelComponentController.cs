using DCL;
using System;

public class JoinChannelComponentController : IDisposable
{
    private IJoinChannelComponentView joinChannelView;
    private DataStore_Channels channelsDataStore;

    public JoinChannelComponentController(DataStore_Channels channelsDataStore)
    {
        joinChannelView = CreateJoinChannelView();
        this.channelsDataStore = channelsDataStore;

        this.channelsDataStore.currentJoinChannelModal.OnChange += OnChannelToJoinChanged;
        joinChannelView.OnCancelJoin += OnCancelJoin;
        joinChannelView.OnConfirmJoin += OnConfirmJoin;
    }

    public void Dispose()
    {
        channelsDataStore.currentJoinChannelModal.OnChange -= OnChannelToJoinChanged;
    }

    private void OnChannelToJoinChanged(string currentChannelId, string previousChannelId)
    {
        if (string.IsNullOrEmpty(currentChannelId))
            return;

        joinChannelView.SetChannel(currentChannelId);
        joinChannelView.Show();
    }

    private void OnCancelJoin()
    {
        joinChannelView.Hide();
        channelsDataStore.currentJoinChannelModal.Set(null);
    }

    private void OnConfirmJoin(string channelId)
    {
        joinChannelView.Hide();
        channelsDataStore.currentJoinChannelModal.Set(null);
    }

    protected internal virtual IJoinChannelComponentView CreateJoinChannelView() => JoinChannelComponentView.Create();
}
