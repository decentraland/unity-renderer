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
        void SendClickedOnCollectibles();
        void SendStartedConversation(PlayerActionSource source, string recipientId);
        void SendNftBuy(PlayerActionSource source);
        void SendInspectAvatar(double timeSpent);
        void SendLinkClick(PlayerActionSource source);
        void SendCopyWallet(PlayerActionSource source);
        void SendCopyUsername(PlayerActionSource source);
        void SendJumpInToPlayer(PlayerActionSource source, string recipientId);
        void SendProfileEdit(int descriptionLength, bool hasLinks, PlayerActionSource source);
        void SendVoiceChatPreferencesChanged(VoiceChatAllow preference);
        void SendFriendRequestSent(string fromUserId, string toUserId, double messageLength, PlayerActionSource source, string friendRequestId);
        void SendFriendRequestApproved(string fromUserId, string toUserId, string source, bool hasBodyMessage, string friendRequestId);
        void SendFriendRequestRejected(string fromUserId, string toUserId, string source, bool hasBodyMessage, string friendRequestId);
        void SendFriendRequestCancelled(string fromUserId, string toUserId, string source, string friendRequestId);
        void SendFriendDeleted(string fromUserId, string toUserId, PlayerActionSource source);
        void SendMessageWithMention(string recipientId);
        void SendClickedMention(string recipientId);
        void SendMentionCreated(MentionCreationSource source, string recipientId);
        void SendPassportOpen(string recipientId, bool found = true, AvatarOpenSource source = AvatarOpenSource.World);
        void SendPassportClose(string recipientId, double timeSpent);
        void SendPlayerBlocked(bool isFriend, PlayerActionSource source, string recipientId);
        void SendPlayerUnblocked(bool isFriend, PlayerActionSource source, string recipientId);
        void SendPlayerReport(PlayerReportIssueType issueType, double messageLength, PlayerActionSource source, string recipientId);
        void SendPlayerJoin(PlayerActionSource source, string recipientId);
        void SendPlayEmote(string emoteId, string emoteName, string rarity, bool isBaseEmote, UserProfile.EmoteSource source, string parcelLocation);
        void SendEmptyChannelCreated(string channelName, ChannelJoinedSource source);
        void SendPopulatedChannelJoined(string channelName, ChannelJoinedSource source, string method);
        void SendLeaveChannel(string channelId, ChannelLeaveSource source);
        void SendChannelSearch(string text);
        void SendChannelLinkClicked(string channel, bool joinAccepted, ChannelLinkSource source);
        void SendFriendRequestError(string senderId, string recipientId, string source, string errorDescription, string friendRequestId);
    }
}
