using DCL.Helpers;

namespace DCL.Chat.HUD
{
    public class PromoteChannelsToastPlugin : IPlugin
    {
        private readonly PromoteChannelsToastComponentController promoteChannelsToastController;

        public PromoteChannelsToastPlugin()
        {
            promoteChannelsToastController = new PromoteChannelsToastComponentController(
                PromoteChannelsToastComponentView.Create(),
                new DefaultPlayerPrefs(),
                DataStore.i,
                CommonScriptableObjects.rendererState);
        }

        public void Dispose() => promoteChannelsToastController.Dispose();
    }
}