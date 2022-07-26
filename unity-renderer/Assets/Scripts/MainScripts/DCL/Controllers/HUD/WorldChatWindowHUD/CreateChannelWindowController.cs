using System;
using DCL.Chat.Channels;

namespace DCL.Chat.HUD
{
    public class CreateChannelWindowController : IHUD
    {
        private readonly IChatController chatController;
        private ICreateChannelWindowView view;
        private string channelName;

        public event Action<string> OnNavigateToChannelWindow;

        public CreateChannelWindowController(IChatController chatController)
        {
            this.chatController = chatController;
        }

        public ICreateChannelWindowView View => view;

        public void Initialize(ICreateChannelWindowView view)
        {
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
                view.ClearInputText();
                view.ClearError();
                view.OnChannelNameUpdated += HandleChannelNameUpdated;
                view.OnCreateSubmit += CreateChannel;
                view.OnClose += HandleViewClose;
                view.OnOpenChannel += HandleOpenChannel;
                chatController.OnJoinChannelError += HandleCreationError;
                chatController.OnChannelJoined += HandleChannelJoined;
            }
            else
            {
                UnsubscribeFromEvents();
                view.Hide();
            }
        }

        private void HandleOpenChannel() => OnNavigateToChannelWindow?.Invoke(channelName);

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

            var channel = chatController.GetAllocatedChannel(channelName);
            if (channel != null)
            {
                view.ShowChannelExistsError(!channel.Joined);
                view.DisableCreateButton();
            }
            else
            {
                view.ClearError();
                view.EnableCreateButton();
            }
        }

        private void CreateChannel()
        {
            chatController.CreateChannel(channelName);
            view.DisableCreateButton();
        }
        
        private void HandleCreationError(string channelId, string message)
        {
            view.ShowError(message);
            view.DisableCreateButton();
        }
    }
}