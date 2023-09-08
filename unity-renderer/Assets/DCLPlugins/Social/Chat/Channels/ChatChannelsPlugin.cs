using System.Collections.Generic;

namespace DCL.Social.Chat.Channels
{
    public class ChatChannelsPlugin : IPlugin
    {
        private readonly List<ChannelLinkDetectorController> channelLinkDetectors = new ();
        private readonly UserProfileWebInterfaceBridge userProfileBridge;

        public ChatChannelsPlugin()
        {
            userProfileBridge = new UserProfileWebInterfaceBridge();

            foreach (var channelLinkDetector in ChannelLinkDetector.INSTANCES.Get())
                OnChannelLinkDetectorAdded(channelLinkDetector);

            ChannelLinkDetector.INSTANCES.OnAdded += OnChannelLinkDetectorAdded;
        }

        public void Dispose()
        {
            ChannelLinkDetector.INSTANCES.OnAdded -= OnChannelLinkDetectorAdded;

            foreach (ChannelLinkDetectorController controller in channelLinkDetectors)
                controller.Dispose();
        }

        private void OnChannelLinkDetectorAdded(ChannelLinkDetector view)
        {
            ChannelLinkDetectorController controller = new ChannelLinkDetectorController(view,
                DataStore.i, userProfileBridge);
            channelLinkDetectors.Add(controller);
        }
    }
}
