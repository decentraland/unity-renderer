using System;

namespace DCL.Chat.HUD
{
    [Serializable]
    public class PublicChatEntryModel : BaseComponentModel
    {
        public string channelId;
        public string name;
        public long lastMessageTimestamp;
        public bool isJoined;
        public int memberCount;
        public bool muted;

        public PublicChatEntryModel(string channelId, string name, long lastMessageTimestamp, bool isJoined, int memberCount, bool muted)
        {
            this.channelId = channelId;
            this.name = name;
            this.lastMessageTimestamp = lastMessageTimestamp;
            this.isJoined = isJoined;
            this.memberCount = memberCount;
            this.muted = muted;
        }
    }
}