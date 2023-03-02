using System;

namespace DCL.Social.Chat.Channels
{
    [Serializable]
    public class JoinChannelComponentModel : BaseComponentModel
    {
        public string channelId;
        public bool isLoading;
    }
}
