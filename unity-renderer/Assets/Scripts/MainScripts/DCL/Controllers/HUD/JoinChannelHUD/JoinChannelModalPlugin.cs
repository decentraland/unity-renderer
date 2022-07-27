
using DCL;
/// <summary>
/// Plugin feature that initialize the Join Channel Modal feature.
/// </summary>
public class JoinChannelModalPlugin : IPlugin
{
    public JoinChannelComponentController joinChannelComponentController;

    public JoinChannelModalPlugin() { joinChannelComponentController = new JoinChannelComponentController(DataStore.i.channels); }

    public void Dispose() { joinChannelComponentController.Dispose(); }
}
