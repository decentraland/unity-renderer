namespace DCL.Chat.HUD
{
    public class ChannelLeaveErrorWindowPlugin : IPlugin
    {
        private readonly ChannelLeaveErrorWindowController controller;

        public ChannelLeaveErrorWindowPlugin()
        {
            controller = new ChannelLeaveErrorWindowController(
                ChannelLeaveErrorWindowComponentView.Create(),
                ChatController.i,
                DataStore.i);
        }
        
        public void Dispose() => controller.Dispose();
    }
}