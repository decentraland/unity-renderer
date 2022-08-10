using System;

namespace DCL.Chat.HUD
{
    public class LeaveChannelConfirmationWindowController : IHUD
    {
        internal ILeaveChannelConfirmationWindowComponentView joinChannelView;
        internal IChatController chatController;

        public event Action<string> OnLeaveChannel;

        public LeaveChannelConfirmationWindowController(IChatController chatController)
        {
            this.chatController = chatController;

            this.chatController.OnChannelLeft += HandleChannelLeft;
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
            OnLeaveChannel?.Invoke(channelId);
            joinChannelView.Hide();
        }
    }
}