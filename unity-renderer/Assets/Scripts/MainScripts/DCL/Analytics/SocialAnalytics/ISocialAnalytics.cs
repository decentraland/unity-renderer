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
        void SendStartedConversation(PlayerActionSource source);
        void SendNftBuy(PlayerActionSource source);
        void SendInspectAvatar(double timeSpent);
        void SendLinkClick(PlayerActionSource source);
        void SendCopyWallet(PlayerActionSource source);
        void SendCopyUsername(PlayerActionSource source);
        void SendJumpInToPlayer(PlayerActionSource source);
        void SendProfileEdit(int descriptionLength, bool hasLinks, PlayerActionSource source);
        void SendVoiceChatPreferencesChanged(VoiceChatAllow preference);
        void SendFriendRequestSent(string fromUserId, string toUserId, double messageLength, PlayerActionSource source, string friendRequestId);
        void SendFriendRequestApproved(string fromUserId, string toUserId, string source, bool hasBodyMessage, string friendRequestId);
        void SendFriendRequestRejected(string fromUserId, string toUserId, string source, bool hasBodyMessage, string friendRequestId);
        void SendFriendRequestCancelled(string fromUserId, string toUserId, string source, string friendRequestId);
        void SendFriendDeleted(string fromUserId, string toUserId, PlayerActionSource source);
        void SendMessageWithMention();
        void SendClickedMention();
        void SendMentionCreated(MentionCreationSource source);
        void SendPassportOpen(bool found = true, AvatarOpenSource source = AvatarOpenSource.World);
        void SendPassportClose(double timeSpent);
        void SendPlayerBlocked(bool isFriend, PlayerActionSource source);
        void SendPlayerUnblocked(bool isFriend, PlayerActionSource source);
        void SendPlayerReport(PlayerReportIssueType issueType, double messageLength, PlayerActionSource source);
        void SendPlayerJoin(PlayerActionSource source);
        void SendPlayEmote(string emoteId, string emoteName, string rarity, bool isBaseEmote, UserProfile.EmoteSource source, string parcelLocation);
        void SendEmptyChannelCreated(string channelName, ChannelJoinedSource source);
        void SendPopulatedChannelJoined(string channelName, ChannelJoinedSource source, string method);
        void SendLeaveChannel(string channelId, ChannelLeaveSource source);
        void SendChannelSearch(string text);
        void SendChannelLinkClicked(string channel, bool joinAccepted, ChannelLinkSource source);
        void SendFriendRequestError(string senderId, string recipientId, string source, string errorDescription, string friendRequestId);
    }
}
