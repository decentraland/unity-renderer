namespace SocialFeaturesAnalytics
{
    public interface ISocialAnalytics
    {
        void SendPlayerMuted(string toUserId);
        void SendPlayerUnmuted(string toUserId);
        void SendVoiceMessageStartedByFirstTime();
        void SendVoiceMessageSent(double messageLength);
        void SendChannelMessageSent(string fromUserId, double messageLength, string channel, ChatMessageType messageType);
        void SendChannelMessageReceived(string fromUserId, double messageLength, string channel, ChatMessageType messageType);
        void SendDirectMessageSent(string fromUserId, string toUserId, double messageLength, bool areFriends, ChatContentType contentType);
        void SendDirectMessageReceived(string fromUserId, string toUserId, double messageLength, bool areFriends, ChatContentType contentType);
        void SendFriendRequestSent(string fromUserId, string toUserId, double messageLength, FriendActionSource source);
        void SendFriendRequestApproved(string fromUserId, string toUserId, FriendActionSource source);
        void SendFriendRequestRejected(string fromUserId, string toUserId, FriendActionSource source);
        void SendFriendRequestCancelled(string fromUserId, string toUserId, FriendActionSource source);
        void SendFriendDeleted(string fromUserId, string toUserId, FriendActionSource source);
        void SendPassportOpen();
        void SendPassportClose(double timeSpent);
        void SendPlayerBlocked(bool isFriend, FriendActionSource source);
        void SendPlayerUnblocked(bool isFriend, FriendActionSource source);
        void SendPlayerReport(PlayerReportIssueType issueType, double messageLength, FriendActionSource source);
        void SendPlayerJoin(FriendActionSource source);
        void SendPlayEmote(string emoteName, string rarity, EmoteSource source);
    }
}