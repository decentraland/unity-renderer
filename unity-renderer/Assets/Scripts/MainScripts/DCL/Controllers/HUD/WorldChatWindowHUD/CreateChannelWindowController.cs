using System;
using System.Text.RegularExpressions;
using DCL.Chat.Channels;

namespace DCL.Chat.HUD
{
    public class CreateChannelWindowController : IHUD
    {
        private readonly IChatController chatController;
        private readonly DataStore dataStore;
        private readonly Regex nameFormatRegex = new Regex("^[a-zA-Z0-9-]{3,20}$");
        private ICreateChannelWindowView view;
        private string channelName;

        public event Action<string> OnNavigateToChannelWindow;

        public CreateChannelWindowController(IChatController chatController,
            DataStore dataStore)
        {
            this.chatController = chatController;
            this.dataStore = dataStore;
        }

        public ICreateChannelWindowView View => view;

        public void Initialize(ICreateChannelWindowView view)
        {
            this.view = view;
        }

        public void Dispose()
        {
            UnsubscribeFromEvents();
            view.Dispose();
        }

        public void SetVisibility(bool visible)
        {
            if (visible)
            {
                view.Show();
                view.ClearInputText();
                view.FocusInputField();
                view.ClearError();
                view.DisableCreateButton();
                view.OnChannelNameUpdated += HandleChannelNameUpdated;
                view.OnCreateSubmit += CreateChannel;
                view.OnClose += HandleViewClose;
                view.OnJoinChannel += HandleJoinChannel;
                chatController.OnJoinChannelError += HandleCreationError;
                chatController.OnChannelJoined += HandleChannelJoined;
                dataStore.channels.isCreationModalVisible.Set(true);
            }
            else
            {
                UnsubscribeFromEvents();
                view.Hide();
                dataStore.channels.isCreationModalVisible.Set(false);
            }
        }

        private void HandleJoinChannel() => chatController.JoinOrCreateChannel(channelName);

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
            channelName = name.ToLower();

            var channel = chatController.GetAllocatedChannel(channelName);
            if (channel != null)
            {
                view.ShowChannelExistsError(!channel.Joined);
                view.DisableCreateButton();
            }
            else if (name.Length == 0)
                view.DisableCreateButton();
            else if (name.Length < 3)
            {
                view.ShowTooShortError();
                view.DisableCreateButton();
            }
            else if (!nameFormatRegex.IsMatch(name))
            {
                view.ShowWrongFormatError();
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
            if (string.IsNullOrEmpty(channelName)) return;
            if (!nameFormatRegex.IsMatch(channelName)) return;
            if (chatController.GetAllocatedChannel(channelName) != null) return;
            chatController.CreateChannel(channelName);
            view.DisableCreateButton();
        }

        private void HandleCreationError(string channelId, ChannelErrorCode errorCode)
        {
            switch (errorCode)
            {
                case ChannelErrorCode.LimitExceeded:
                    view.ShowChannelsExceededError();
                    break;
                case ChannelErrorCode.WrongFormat:
                    view.ShowWrongFormatError();
                    break;
            }
            
            view.DisableCreateButton();
        }
    }
}