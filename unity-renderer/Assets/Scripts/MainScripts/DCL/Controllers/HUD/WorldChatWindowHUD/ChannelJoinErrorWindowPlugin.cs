using UnityEngine;

namespace DCL.Chat.HUD
{
    public class ChannelJoinErrorWindowPlugin : IPlugin
    {
        private readonly ChannelJoinErrorWindowController channelLimitReachedWindow;

        public ChannelJoinErrorWindowPlugin()
        {
            channelLimitReachedWindow = new ChannelJoinErrorWindowController(
                GameObject.Instantiate(Resources.Load<ChannelJoinErrorWindowComponentView>("SocialBarV1/ChannelJoinErrorModal")),
                Environment.i.serviceLocator.Get<IChatController>(),
                DataStore.i);
        }

        public void Dispose() => channelLimitReachedWindow.Dispose();
    }
}
