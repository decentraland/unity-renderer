namespace DCL.Chat
{
    public interface IChannelsFeatureFlagService
    {
        bool IsChannelsFeatureEnabled();
        bool IsAllowedToCreateChannels();
    }
}