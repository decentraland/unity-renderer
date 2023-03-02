namespace DCL.Chat.HUD
{
    public class ChannelLeaveErrorWindowPlugin : IPlugin
    {
        private readonly ChannelLeaveErrorWindowController controller;

        public ChannelLeaveErrorWindowPlugin()
        {
            controller = new ChannelLeaveErrorWindowController(
                ChannelLeaveErrorWindowComponentView.Create(),
                Environment.i.serviceLocator.Get<IChatController>(),
                DataStore.i);
        }

        public void Dispose() => controller.Dispose();
    }
}
