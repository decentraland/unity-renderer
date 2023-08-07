using System;

namespace DCL.Social.Chat
{
    [Serializable]
    public class PublicChatModel : BaseComponentModel
    {
        public string channelId;
        public string name;
        public string description;
        public bool joined;
        public int memberCount;
        public bool muted;
        public bool showOnlyOnlineMembers;

        public PublicChatModel(string channelId, string name, string description, bool joined,
            int memberCount, bool muted, bool showOnlyOnlineMembers)
        {
            this.channelId = channelId;
            this.name = name;
            this.description = description;
            this.joined = joined;
            this.memberCount = memberCount;
            this.muted = muted;
            this.showOnlyOnlineMembers = showOnlyOnlineMembers;
        }

        public void CopyFrom(PublicChatModel model)
        {
            channelId = model.channelId;
            name = model.name;
            description = model.description;
            joined = model.joined;
            memberCount = model.memberCount;
            muted = model.muted;
        }
    }
}
