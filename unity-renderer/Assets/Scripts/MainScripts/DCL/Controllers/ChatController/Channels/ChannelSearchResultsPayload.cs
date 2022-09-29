using System;
using JetBrains.Annotations;

namespace DCL.Chat.Channels
{
    [Serializable]
    public class ChannelSearchResultsPayload
    {
        [CanBeNull] public string since;
        public ChannelInfoPayloads channels;
    }
}