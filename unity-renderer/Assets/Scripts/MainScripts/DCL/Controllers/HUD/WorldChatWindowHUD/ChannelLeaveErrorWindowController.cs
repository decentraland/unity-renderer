namespace DCL.Chat.HUD
{
    public class ChannelLeaveErrorWindowController
    {
        private readonly IChannelLeaveErrorWindowView view;
        private readonly IChatController chatController;
        private readonly DataStore dataStore;
        private string currentChannelId;

        public ChannelLeaveErrorWindowController(IChannelLeaveErrorWindowView view,
            IChatController chatController,
            DataStore dataStore)
        {
            this.view = view;
            this.chatController = chatController;
            view.Hide();
            view.OnClose += Close;
            view.OnRetry += Retry;
            this.dataStore = dataStore;
            dataStore.channels.leaveChannelError.OnChange += Show;
        }

        public void Dispose()
        {
            dataStore.channels.leaveChannelError.OnChange -= Show;

            if (view != null)
            {
                view.OnClose -= Close;
                view.OnRetry -= Retry;
                view.Dispose();
            }
        }

        private void Show(string currentChannelId, string previousChannelId)
        {
            if (string.IsNullOrEmpty(currentChannelId)) return;
            this.currentChannelId = currentChannelId;
            var channel = chatController.GetAllocatedChannel(currentChannelId);
            view.Show(channel?.Name ?? currentChannelId);
        }

        private void Close() => view.Hide();
        
        private void Retry()
        {
            chatController.LeaveChannel(currentChannelId);
            Close();
        }
    }
}