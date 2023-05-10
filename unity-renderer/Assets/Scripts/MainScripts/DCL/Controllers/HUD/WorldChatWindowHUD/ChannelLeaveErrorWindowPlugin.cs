using UnityEngine;

namespace DCL.Chat.HUD
{
    public class ChannelLeaveErrorWindowPlugin : IPlugin
    {
        private readonly ChannelLeaveErrorWindowController controller;

        public ChannelLeaveErrorWindowPlugin()
        {
            controller = new ChannelLeaveErrorWindowController(
                GameObject.Instantiate(Resources.Load<ChannelLeaveErrorWindowComponentView>("SocialBarV1/ChannelLeaveErrorModal")),
            Environment.i.serviceLocator.Get<IChatController>(),
                DataStore.i);
        }

        public void Dispose() => controller.Dispose();
    }
}
