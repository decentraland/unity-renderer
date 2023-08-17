using System;

namespace DCL.Social.Chat
{
    [Serializable]
    public class PublicChatEntryModel : BaseComponentModel
    {
        public string channelId;
        public string name;
        public bool isJoined;
        public int memberCount;
        public bool showOnlyOnlineMembers;
        public bool muted;

        public PublicChatEntryModel(string channelId, string name, bool isJoined, int memberCount, bool showOnlyOnlineMembers, bool muted)
        {
            this.channelId = channelId;
            this.name = name;
            this.isJoined = isJoined;
            this.memberCount = memberCount;
            this.showOnlyOnlineMembers = showOnlyOnlineMembers;
            this.muted = muted;
        }
    }
}
