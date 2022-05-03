namespace SocialAnalytics
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
        void SendReloadDirectMessages();
        void SendFriendRequestSent(PlayerType fromPlayerType, PlayerType toPlayerType, double messageLength, FriendActionSource source);
        void SendFriendRequestApproved(PlayerType fromPlayerType, PlayerType toPlayerType, FriendActionSource source);
        void SendFriendRequestRejected(PlayerType fromPlayerType, PlayerType toPlayerType, FriendActionSource source);
        void SendFriendRequestCancelled(PlayerType fromPlayerType, PlayerType toPlayerType, FriendActionSource source);
        void SendFriendRequestReceived(PlayerType fromPlayerType, PlayerType toPlayerType);
        void SendReloadFriends();
        void SendPassportOpen();
        void SendPassportClose(double timeSpent);
        void SendPlayerBlocked(bool isFriend, FriendActionSource source);
        void SendPlayerUnblocked(bool isFriend, FriendActionSource source);
        void SendPlayerReport(PlayerReportIssueType issueType, double messageLength);
        void SendPlayerJoin(FriendActionSource source);
        void SendPlayEmote(string emoteName, string rarity, EmoteSource source);
    }
}