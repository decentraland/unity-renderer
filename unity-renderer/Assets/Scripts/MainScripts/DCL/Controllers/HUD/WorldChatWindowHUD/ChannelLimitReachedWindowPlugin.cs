using UnityEngine;

namespace DCL.Chat.HUD
{
    public class ChannelLimitReachedWindowPlugin : IPlugin
    {
        private readonly ChannelLimitReachedWindowController channelLimitReachedWindow;

        public ChannelLimitReachedWindowPlugin()
        {
            channelLimitReachedWindow = new ChannelLimitReachedWindowController(
                GameObject.Instantiate(Resources.Load<ChannelLimitReachedWindowComponentView>("SocialBarV1/ChannelLimitReachedModal")),
                DataStore.i);
        }

        public void Dispose() => channelLimitReachedWindow.Dispose();
    }
}
