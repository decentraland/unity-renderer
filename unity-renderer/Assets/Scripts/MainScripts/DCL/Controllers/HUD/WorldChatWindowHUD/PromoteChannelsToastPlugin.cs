namespace DCL.Chat.HUD
{
    public class PromoteChannelsToastPlugin : IPlugin
    {
        private readonly PromoteChannelsToastComponentController promoteChannelsToastController;

        public PromoteChannelsToastPlugin()
        {
            promoteChannelsToastController = new PromoteChannelsToastComponentController(
                PromoteChannelsToastComponentView.Create(),
                DataStore.i);
        }

        public void Dispose() => promoteChannelsToastController.Dispose();
    }
}