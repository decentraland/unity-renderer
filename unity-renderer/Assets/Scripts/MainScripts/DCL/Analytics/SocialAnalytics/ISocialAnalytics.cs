namespace SocialFeaturesAnalytics
{
    public interface ISocialAnalytics
    {
        void SendPlayerMuted(PlayerType toPlayerType);
        void SendPlayerUnmuted(PlayerType toPlayerType);
        void SendVoiceMessageSent(double messageLength);
        void SendChannelMessageSent(PlayerType fromPlayerType, double messageLength, string channel, ChatMessageType messageType);
        void SendChannelMessageReceived(PlayerType fromPlayerType, double messageLength, string channel, ChatMessageType messageType);
        void SendDirectMessageSent(PlayerType fromPlayerType, PlayerType toPlayerType, double messageLength, bool areFriends, ChatContentType contentType);
        void SendDirectMessageReceived(PlayerType fromPlayerType, PlayerType toPlayerType, double messageLength, bool areFriends, ChatContentType contentType);
        void SendFriendRequestSent(string fromUserId, string toUserId, double messageLength, FriendActionSource source);
        void SendFriendRequestApproved(string fromUserId, string toUserId, FriendActionSource source);
        void SendFriendRequestRejected(string fromUserId, string toUserId, FriendActionSource source);
        void SendFriendRequestCancelled(string fromUserId, string toUserId, FriendActionSource source);
        void SendFriendDeleted(string fromUserId, string toUserId, FriendActionSource source);
        void SendPassportOpen();
        void SendPassportClose(double timeSpent);
        void SendPlayerBlocked(bool isFriend, FriendActionSource source);
        void SendPlayerUnblocked(bool isFriend, FriendActionSource source);
        void SendPlayerReport(PlayerReportIssueType issueType, double messageLength);
        void SendPlayerJoin(FriendActionSource source);
        void SendPlayEmote(string emoteName, string rarity, EmoteSource source);
    }
}