using DCL;
using System;

public class JoinChannelComponentController : IDisposable
{
    internal readonly IJoinChannelComponentView joinChannelView;
    internal readonly IChatController chatController;
    internal readonly DataStore_Channels channelsDataStore;

    public JoinChannelComponentController(
        IJoinChannelComponentView joinChannelView,
        IChatController chatController,
        DataStore_Channels channelsDataStore)
    {
        this.joinChannelView = joinChannelView;
        this.chatController = chatController;
        this.channelsDataStore = channelsDataStore;

        this.channelsDataStore.currentJoinChannelModal.OnChange += OnChannelToJoinChanged;
        this.joinChannelView.OnCancelJoin += OnCancelJoin;
        this.joinChannelView.OnConfirmJoin += OnConfirmJoin;
    }

    public void Dispose()
    {
        channelsDataStore.currentJoinChannelModal.OnChange -= OnChannelToJoinChanged;
        joinChannelView.OnCancelJoin -= OnCancelJoin;
        joinChannelView.OnConfirmJoin -= OnConfirmJoin;
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
    }

    private void OnConfirmJoin(string channelId)
    {
        chatController.JoinOrCreateChannel(channelId.Replace("#", "").Replace("~", ""));
        joinChannelView.Hide();
        channelsDataStore.currentJoinChannelModal.Set(null);
    }
}
