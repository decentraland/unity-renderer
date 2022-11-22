using System;
using DCL.Chat.Channels;

namespace DCL.Chat
{
    public interface IChannelsFeatureFlagService : IService
    {
        event Action<bool> OnAllowedToCreateChannelsChanged;

        bool IsChannelsFeatureEnabled();
        bool IsAllowedToCreateChannels();
        AutomaticJoinChannelList GetAutoJoinChannelsList();
        bool IsPromoteChannelsToastEnabled();
    }
}