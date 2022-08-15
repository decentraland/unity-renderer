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
            JoinChannelComponentView.Create(),
            // TODO (channels): Pass ChatController.i after kernel integration
            ChatChannelsControllerMock.i,
            DataStore.i.channels);
    }

    public void Dispose() { joinChannelComponentController.Dispose(); }
}
