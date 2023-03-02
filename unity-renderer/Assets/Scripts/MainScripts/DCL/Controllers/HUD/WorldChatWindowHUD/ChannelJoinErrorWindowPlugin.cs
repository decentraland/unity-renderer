namespace DCL.Chat.HUD
{
    public class ChannelJoinErrorWindowPlugin : IPlugin
    {
        private readonly ChannelJoinErrorWindowController channelLimitReachedWindow;

        public ChannelJoinErrorWindowPlugin()
        {
            channelLimitReachedWindow = new ChannelJoinErrorWindowController(
                ChannelJoinErrorWindowComponentView.Create(),
                Environment.i.serviceLocator.Get<IChatController>(),
                DataStore.i);
        }

        public void Dispose() => channelLimitReachedWindow.Dispose();
    }
}
