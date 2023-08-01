using DCL.Interface;

namespace DCL.Social.Chat
{
    public struct PrivateChatModel
    {
        public string userId;
        public string userName;
        public string faceSnapshotUrl;
        public ChatMessage recentMessage;
        public bool isBlocked;
        public bool isOnline;
    }
}
