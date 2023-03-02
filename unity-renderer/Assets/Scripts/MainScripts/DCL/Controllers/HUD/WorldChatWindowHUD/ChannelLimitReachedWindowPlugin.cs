namespace DCL.Chat.HUD
{
    public class ChannelLimitReachedWindowPlugin : IPlugin
    {
        private readonly ChannelLimitReachedWindowController channelLimitReachedWindow;

        public ChannelLimitReachedWindowPlugin()
        {
            channelLimitReachedWindow = new ChannelLimitReachedWindowController(
                ChannelLimitReachedWindowComponentView.Create(),
                DataStore.i);
        }
        
        public void Dispose() => channelLimitReachedWindow.Dispose();
    }
}