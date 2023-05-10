using DCL.Helpers;
using UnityEngine;

namespace DCL.Chat.HUD
{
    public class PromoteChannelsToastPlugin : IPlugin
    {
        private readonly PromoteChannelsToastComponentController promoteChannelsToastController;

        public PromoteChannelsToastPlugin()
        {
            promoteChannelsToastController = new PromoteChannelsToastComponentController(
                GameObject.Instantiate(Resources.Load<PromoteChannelsToastComponentView>("SocialBarV1/PromoteChannelsHUD")),
                new DefaultPlayerPrefs(),
                DataStore.i,
                CommonScriptableObjects.rendererState);
        }

        public void Dispose() => promoteChannelsToastController.Dispose();
    }
}
