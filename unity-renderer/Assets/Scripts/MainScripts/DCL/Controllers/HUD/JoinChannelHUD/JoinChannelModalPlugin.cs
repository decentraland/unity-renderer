using DCL;
using DCL.Chat.Channels;

/// <summary>
/// Plugin feature that initialize the Join Channel Modal feature.
/// </summary>
public class JoinChannelModalPlugin : IPlugin
{
    public JoinChannelComponentController joinChannelComponentController;

    public JoinChannelModalPlugin()
    {
        joinChannelComponentController = new JoinChannelComponentController(
            // TODO (channels): Pass ChatController.i after kernel integration
            new ChatChannelsControllerMock(ChatController.i, UserProfileController.i),
            DataStore.i.channels);
    }

    public void Dispose() { joinChannelComponentController.Dispose(); }
}
