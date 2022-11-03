using System;

namespace DCL.Chat
{
    public interface IChannelsFeatureFlagService : IService
    {
        event Action<bool> OnAllowedToCreateChannelsChanged;
        bool IsChannelsFeatureEnabled();
        bool IsAllowedToCreateChannels();
        bool IsPromoteChannelsToastEnabled();
    }
}