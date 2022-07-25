using System;
using DCL.Chat.Channels;

namespace DCL.Chat.HUD
{
    public class CreateChannelWindowController : IHUD
    {
        private readonly IChatController chatController;
        private readonly ICreateChannelWindowView view;
        private string channelName;

        public event Action<string> OnNavigateToChannelWindow;

        public CreateChannelWindowController(IChatController chatController,
            ICreateChannelWindowView view)
        {
            this.chatController = chatController;
            this.view = view;
        }

        public void Dispose()
        {
            UnsubscribeFromEvents();
        }

        public void SetVisibility(bool visible)
        {
            if (visible)
            {
                view.Show();
                view.OnChannelNameUpdated += HandleChannelNameUpdated;
                view.OnCreateSubmit += CreateChannel;
                view.OnClose += HandleViewClose;
                chatController.OnJoinChannelError += HandleCreationError;
                chatController.OnChannelJoined += HandleChannelJoined;
            }
            else
            {
                UnsubscribeFromEvents();
                view.Hide();
            }
        }

        private void HandleViewClose() => SetVisibility(false);

        private void HandleChannelJoined(Channel channel) => OnNavigateToChannelWindow?.Invoke(channel.ChannelId);

        private void UnsubscribeFromEvents()
        {
            view.OnChannelNameUpdated -= HandleChannelNameUpdated;
            view.OnCreateSubmit -= CreateChannel;
            view.OnClose -= HandleViewClose;
            chatController.OnJoinChannelError -= HandleCreationError;
            chatController.OnChannelJoined -= HandleChannelJoined;
        }

        private void HandleChannelNameUpdated(string name)
        {
            channelName = name;

            if (chatController.GetAllocatedChannel(channelName) != null)
            {
                // TODO: confirm fail flow
            }
        }

        private void CreateChannel()
        {
            chatController.CreateChannel(channelName);
        }
        
        private void HandleCreationError(string channelId, string message) => view.ShowError(message);
    }
}