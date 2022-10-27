using DCL;
using static DCL.SettingsCommon.GeneralSettings;

namespace SocialFeaturesAnalytics
{
    public interface ISocialAnalytics
    {
        void SendPlayerMuted(string toUserId);
        void SendPlayerUnmuted(string toUserId);
        void SendVoiceMessageStartedByFirstTime();
        void SendVoiceMessage(double messageLength, VoiceMessageSource source, string fromUserId);
        void SendVoiceChannelConnection(int numberOfPeers);
        void SendVoiceChannelDisconnection();
        void SendVoiceChatPreferencesChanged(VoiceChatAllow preference);
        void SendFriendRequestSent(string fromUserId, string toUserId, double messageLength, PlayerActionSource source);
        void SendFriendRequestApproved(string fromUserId, string toUserId, PlayerActionSource source);
        void SendFriendRequestRejected(string fromUserId, string toUserId, PlayerActionSource source);
        void SendFriendRequestCancelled(string fromUserId, string toUserId, PlayerActionSource source);
        void SendFriendDeleted(string fromUserId, string toUserId, PlayerActionSource source);
        void SendPassportOpen();
        void SendPassportClose(double timeSpent);
        void SendPlayerBlocked(bool isFriend, PlayerActionSource source);
        void SendPlayerUnblocked(bool isFriend, PlayerActionSource source);
        void SendPlayerReport(PlayerReportIssueType issueType, double messageLength, PlayerActionSource source);
        void SendPlayerJoin(PlayerActionSource source);
        void SendPlayEmote(string emoteId, string emoteName, string rarity, bool isBaseEmote, UserProfile.EmoteSource source, string parcelLocation);
        void SendEmptyChannelCreated(string channelName, ChannelJoinedSource source);
        void SendPopulatedChannelJoined(string channelName, ChannelJoinedSource source);
        void SendLeaveChannel(string channelId, ChannelLeaveSource source);
        void SendChannelSearch(string text);
        void SendMessageSentToChannel(string channelName, int bodyLength, string source);
        void SendChannelLinkClicked(string channel, bool joinAccepted, ChannelLinkSource source);
    }
}