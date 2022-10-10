namespace DCL.Chat
{
    public interface IChannelsUtils
    {
        bool IsChannelsFeatureEnabled();
        bool IsAllowedToCreateChannels();
    }
}