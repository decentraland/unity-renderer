using DCL.Chat.Channels;

namespace DCL.Chat.HUD
{
    public class LeaveChannelConfirmationWindowController : IHUD
    {
        internal ILeaveChannelConfirmationWindowComponentView joinChannelView;
        internal readonly IChatController chatController;

        public LeaveChannelConfirmationWindowController(IChatController chatController)
        {
            this.chatController = chatController;

            this.chatController.OnChannelLeft += HandleChannelLeft;
            this.chatController.OnChannelLeaveError += HandleChannelLeaveError;
        }

        public ILeaveChannelConfirmationWindowComponentView View => joinChannelView;

        public void Initialize(ILeaveChannelConfirmationWindowComponentView view)
        {
            joinChannelView = view;

            joinChannelView.OnCancelLeave += OnCancelLeave;
            joinChannelView.OnConfirmLeave += OnConfirmLeave;
        }

        public void Dispose()
        {
            chatController.OnChannelLeft -= HandleChannelLeft;
            chatController.OnChannelLeaveError -= HandleChannelLeaveError;
            joinChannelView.OnCancelLeave -= OnCancelLeave;
            joinChannelView.OnConfirmLeave -= OnConfirmLeave;
        }

        public void SetChannelToLeave(string channelId)
        {
            joinChannelView.SetChannel(channelId);
        }

        public void SetVisibility(bool visible)
        {
            if (visible)
                joinChannelView.Show();
            else
                joinChannelView.Hide();
        }

        private void OnCancelLeave()
        {
            joinChannelView.Hide();
        }

        private void OnConfirmLeave(string channelId)
        {
            chatController.LeaveChannel(channelId);
        }

        private void HandleChannelLeft(string channelId)
        {
            joinChannelView.Hide();
        }
        
        private void HandleChannelLeaveError(string channelId, ChannelErrorCode errorCode) =>
            joinChannelView.Hide();
    }
}