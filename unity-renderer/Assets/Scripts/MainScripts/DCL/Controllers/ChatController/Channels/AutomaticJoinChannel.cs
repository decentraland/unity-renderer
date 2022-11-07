using System;
using System.Collections.Generic;

namespace DCL.Chat.Channels
{
    [Serializable]
    public class AutomaticJoinChannelList
    {
        public AutomaticJoinChannel[] automaticJoinChannelList;
    }

    [Serializable]
    public class AutomaticJoinChannel
    {
        public string channelId;
        public bool enableNotifications;
    }
}