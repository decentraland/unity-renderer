using System;

namespace DCL.Chat
{
    public interface IChannelsFeatureFlagService
    {
        event Action<bool> OnAllowedToCreateChannelsChanged;
        bool IsChannelsFeatureEnabled();
    }
}