using System;

namespace DCL.Chat.HUD
{
    [Serializable]
    public class PrivateChatEntryModel : BaseComponentModel
    {
        public string userId;
        public string userName;
        public string lastMessage;
        public ulong lastMessageTimestamp;
        public string pictureUrl;
        public bool isBlocked;
        public bool isOnline;
        public bool imageFetchingEnabled;

        public PrivateChatEntryModel(string userId, string userName, string lastMessage, string pictureUrl, bool isBlocked, bool isOnline,
            ulong lastMessageTimestamp)
        {
            this.userId = userId;
            this.userName = userName;
            this.lastMessage = lastMessage;
            this.pictureUrl = pictureUrl;
            this.isBlocked = isBlocked;
            this.isOnline = isOnline;
            this.lastMessageTimestamp = lastMessageTimestamp;
        }
    }
}