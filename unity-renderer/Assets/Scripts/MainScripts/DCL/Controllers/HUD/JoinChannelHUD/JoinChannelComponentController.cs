using DCL;
using System;

public class JoinChannelComponentController : IDisposable
{
    internal IJoinChannelComponentView joinChannelView;
    internal IChatController chatController;
    internal DataStore_Channels channelsDataStore;

    public JoinChannelComponentController() { }

    public JoinChannelComponentController(
        IChatController chatController,
        DataStore_Channels channelsDataStore)
    {
        Initialize(chatController, channelsDataStore);
    }

    public void Initialize(
        IChatController chatController,
        DataStore_Channels channelsDataStore)
    {
        joinChannelView = CreateJoinChannelView();
        this.chatController = chatController;
        this.channelsDataStore = channelsDataStore;

        this.channelsDataStore.currentJoinChannelModal.OnChange += OnChannelToJoinChanged;
        joinChannelView.OnCancelJoin += OnCancelJoin;
        joinChannelView.OnConfirmJoin += OnConfirmJoin;
    }

    public void Dispose()
    {
        channelsDataStore.currentJoinChannelModal.OnChange -= OnChannelToJoinChanged;
        joinChannelView.OnCancelJoin -= OnCancelJoin;
        joinChannelView.OnConfirmJoin -= OnConfirmJoin;
    }

    internal void OnChannelToJoinChanged(string currentChannelId, string previousChannelId)
    {
        if (string.IsNullOrEmpty(currentChannelId))
            return;

        joinChannelView.SetChannel(currentChannelId);
        joinChannelView.Show();
    }

    internal void OnCancelJoin()
    {
        joinChannelView.Hide();
        channelsDataStore.currentJoinChannelModal.Set(null);
    }

    internal void OnConfirmJoin(string channelId)
    {
        chatController.JoinOrCreateChannel(channelId.Replace("#", "").Replace("~", ""));
        joinChannelView.Hide();
        channelsDataStore.currentJoinChannelModal.Set(null);
    }

    protected internal virtual IJoinChannelComponentView CreateJoinChannelView() => JoinChannelComponentView.Create();
}
