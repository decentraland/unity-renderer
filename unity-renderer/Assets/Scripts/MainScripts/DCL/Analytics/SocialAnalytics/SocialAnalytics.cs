using NSubstitute;
using System.Collections.Generic;

namespace SocialFeaturesAnalytics
{
    public class SocialAnalytics : ISocialAnalytics
    {
        private const string PLAYER_MUTED = "user_muted";
        private const string PLAYER_UNMUTED = "user_unmuted";
        private const string VOICE_MESSAGE_SENT = "voice_message_sent";
        private const string CHANNEL_MESSAGE_SENT = "send_chat_message";
        private const string CHANNEL_MESSAGE_RECEIVED = "chat message received";
        private const string DIRECT_MESSAGE_SENT = "send_direct_message";
        private const string DIRECT_MESSAGE_RECEIVED = "direct_message_received";
        private const string FRIEND_REQUEST_SENT = "friend_request_sent";
        private const string FRIEND_REQUEST_APPROVED = "friend_request_approved";
        private const string FRIEND_REQUEST_REJECTED = "friend_request_rejected";
        private const string FRIEND_REQUEST_CANCELLED = "friend_request_cancelled";
        private const string FRIEND_REQUEST_RECEIVED = "friend_request_received";
        private const string FRIEND_DELETED = "friend_deleted";
        private const string PASSPORT_OPEN = "passport_open";
        private const string PASSPORT_CLOSE = "passport_close";
        private const string PLAYER_BLOCKED = "user_blocked";
        private const string PLAYER_UNBLOCKED = "user_unblocked";
        private const string PLAYER_REPORT = "player_report";
        private const string PLAYER_JOIN = "player_join";
        private const string PLAY_EMOTE = "play_emote";

        public void SendPlayerMuted(PlayerType toPlayerType)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("to", toPlayerType.ToString());

            GenericAnalytics.SendAnalytic(PLAYER_MUTED, data);
        }

        public void SendPlayerUnmuted(PlayerType toPlayerType)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("to", toPlayerType.ToString());

            GenericAnalytics.SendAnalytic(PLAYER_UNMUTED, data);
        }

        public void SendVoiceMessageSent(double messageLength)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("length", messageLength.ToString());

            GenericAnalytics.SendAnalytic(VOICE_MESSAGE_SENT, data);
        }

        public void SendChannelMessageSent(PlayerType fromPlayerType, double messageLength, string channel, ChatMessageType messageType)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("from", fromPlayerType.ToString());
            data.Add("length", messageLength.ToString());
            data.Add("channel", channel);
            data.Add("message_type", messageType.ToString());

            GenericAnalytics.SendAnalytic(CHANNEL_MESSAGE_SENT, data);
        }

        public void SendChannelMessageReceived(PlayerType fromPlayerType, double messageLength, string channel, ChatMessageType messageType)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("from", fromPlayerType.ToString());
            data.Add("length", messageLength.ToString());
            data.Add("channel", channel);
            data.Add("message_type", messageType.ToString());

            GenericAnalytics.SendAnalytic(CHANNEL_MESSAGE_RECEIVED, data);
        }

        public void SendDirectMessageSent(PlayerType fromPlayerType, PlayerType toPlayerType, double messageLength, bool areFriends, ChatContentType contentType)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("from", fromPlayerType.ToString());
            data.Add("to", toPlayerType.ToString());
            data.Add("length", messageLength.ToString());
            data.Add("friends", areFriends.ToString());
            data.Add("content_type", contentType.ToString());

            GenericAnalytics.SendAnalytic(DIRECT_MESSAGE_SENT, data);
        }

        public void SendDirectMessageReceived(PlayerType fromPlayerType, PlayerType toPlayerType, double messageLength, bool areFriends, ChatContentType contentType)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("from", fromPlayerType.ToString());
            data.Add("to", toPlayerType.ToString());
            data.Add("length", messageLength.ToString());
            data.Add("friends", areFriends.ToString());
            data.Add("content_type", contentType.ToString());

            GenericAnalytics.SendAnalytic(DIRECT_MESSAGE_RECEIVED, data);
        }

        public void SendFriendRequestSent(string fromUserId, string toUserId, double messageLength, FriendActionSource source)
        {
            PlayerType? fromPlayerType = GetPlayerTypeByUserId(fromUserId);
            PlayerType? toPlayerType = GetPlayerTypeByUserId(toUserId);

            if (fromPlayerType == null || toPlayerType == null)
                return;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("from", fromPlayerType.ToString());
            data.Add("to", toPlayerType.ToString());
            data.Add("length", messageLength.ToString());
            data.Add("source", source.ToString());

            GenericAnalytics.SendAnalytic(FRIEND_REQUEST_SENT, data);
        }

        public void SendFriendRequestApproved(string fromUserId, string toUserId, FriendActionSource source)
        {
            PlayerType? fromPlayerType = GetPlayerTypeByUserId(fromUserId);
            PlayerType? toPlayerType = GetPlayerTypeByUserId(toUserId);

            if (fromPlayerType == null || toPlayerType == null)
                return;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("from", fromPlayerType.ToString());
            data.Add("to", toPlayerType.ToString());
            data.Add("source", source.ToString());

            GenericAnalytics.SendAnalytic(FRIEND_REQUEST_APPROVED, data);
        }

        public void SendFriendRequestRejected(string fromUserId, string toUserId, FriendActionSource source)
        {
            PlayerType? fromPlayerType = GetPlayerTypeByUserId(fromUserId);
            PlayerType? toPlayerType = GetPlayerTypeByUserId(toUserId);

            if (fromPlayerType == null || toPlayerType == null)
                return;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("from", fromPlayerType.ToString());
            data.Add("to", toPlayerType.ToString());
            data.Add("source", source.ToString());

            GenericAnalytics.SendAnalytic(FRIEND_REQUEST_REJECTED, data);
        }

        public void SendFriendRequestCancelled(string fromUserId, string toUserId, FriendActionSource source)
        {
            PlayerType? fromPlayerType = GetPlayerTypeByUserId(fromUserId);
            PlayerType? toPlayerType = GetPlayerTypeByUserId(toUserId);

            if (fromPlayerType == null || toPlayerType == null)
                return;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("from", fromPlayerType.ToString());
            data.Add("to", toPlayerType.ToString());
            data.Add("source", source.ToString());

            GenericAnalytics.SendAnalytic(FRIEND_REQUEST_CANCELLED, data);
        }

        public void SendFriendRequestReceived(string fromUserId, string toUserId)
        {
            PlayerType? fromPlayerType = GetPlayerTypeByUserId(fromUserId);
            PlayerType? toPlayerType = GetPlayerTypeByUserId(toUserId);

            if (fromPlayerType == null || toPlayerType == null)
                return;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("from", fromPlayerType.ToString());
            data.Add("to", toPlayerType.ToString());

            GenericAnalytics.SendAnalytic(FRIEND_REQUEST_RECEIVED, data);
        }

        public void SendFriendDeleted(string fromUserId, string toUserId, FriendActionSource source)
        {
            PlayerType? fromPlayerType = GetPlayerTypeByUserId(fromUserId);
            PlayerType? toPlayerType = GetPlayerTypeByUserId(toUserId);

            if (fromPlayerType == null || toPlayerType == null)
                return;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("from", fromPlayerType.ToString());
            data.Add("to", toPlayerType.ToString());
            data.Add("source", source.ToString());

            GenericAnalytics.SendAnalytic(FRIEND_DELETED, data);
        }

        public void SendPassportOpen()
        {
            GenericAnalytics.SendAnalytic(PASSPORT_OPEN);
        }

        public void SendPassportClose(double timeSpent)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("time_spent", timeSpent.ToString());

            GenericAnalytics.SendAnalytic(PASSPORT_CLOSE, data);
        }

        public void SendPlayerBlocked(bool isFriend, FriendActionSource source)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("friend", isFriend.ToString());
            data.Add("source", source.ToString());

            GenericAnalytics.SendAnalytic(PLAYER_BLOCKED, data);
        }

        public void SendPlayerUnblocked(bool isFriend, FriendActionSource source)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("friend", isFriend.ToString());
            data.Add("source", source.ToString());

            GenericAnalytics.SendAnalytic(PLAYER_UNBLOCKED, data);
        }

        public void SendPlayerReport(PlayerReportIssueType issueType, double messageLength)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("issue_type", issueType.ToString());
            data.Add("text_length", messageLength.ToString());

            GenericAnalytics.SendAnalytic(PLAYER_REPORT, data);
        }

        public void SendPlayerJoin(FriendActionSource source)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("source", source.ToString());

            GenericAnalytics.SendAnalytic(PLAYER_JOIN, data);
        }

        public void SendPlayEmote(string emoteName, string rarity, EmoteSource source)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("emote_name", emoteName);
            data.Add("rarity", rarity);
            data.Add("source", source.ToString());

            GenericAnalytics.SendAnalytic(PLAY_EMOTE, data);
        }

        private PlayerType? GetPlayerTypeByUserId(string userId)
        {
            UserProfile userProfile = UserProfileController.GetProfileByUserId(userId);

            if (userProfile == null)
                return null;
            else
                return userProfile.isGuest ? PlayerType.Guest : PlayerType.Wallet;
        }

        public static ISocialAnalytics CreateMockedSocialAnalytics()
        {
            ISocialAnalytics mockedSocialAnalytics = Substitute.For<ISocialAnalytics>();

            mockedSocialAnalytics.When(x => x.SendPlayerMuted(Arg.Any<PlayerType>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendPlayerUnmuted(Arg.Any<PlayerType>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendVoiceMessageSent(Arg.Any<double>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendChannelMessageSent(Arg.Any<PlayerType>(), Arg.Any<double>(), Arg.Any<string>(), Arg.Any<ChatMessageType>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendChannelMessageReceived(Arg.Any<PlayerType>(), Arg.Any<double>(), Arg.Any<string>(), Arg.Any<ChatMessageType>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendDirectMessageSent(Arg.Any<PlayerType>(), Arg.Any<PlayerType>(), Arg.Any<double>(), Arg.Any<bool>(), Arg.Any<ChatContentType>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendDirectMessageReceived(Arg.Any<PlayerType>(), Arg.Any<PlayerType>(), Arg.Any<double>(), Arg.Any<bool>(), Arg.Any<ChatContentType>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendDirectMessageReceived(Arg.Any<PlayerType>(), Arg.Any<PlayerType>(), Arg.Any<double>(), Arg.Any<bool>(), Arg.Any<ChatContentType>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendFriendRequestSent(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<double>(), Arg.Any<FriendActionSource>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendFriendRequestApproved(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<FriendActionSource>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendFriendRequestRejected(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<FriendActionSource>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendFriendRequestCancelled(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<FriendActionSource>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendFriendRequestReceived(Arg.Any<string>(), Arg.Any<string>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendFriendDeleted(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<FriendActionSource>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendPassportOpen()).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendPassportClose(Arg.Any<double>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendPlayerBlocked(Arg.Any<bool>(), Arg.Any<FriendActionSource>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendPlayerUnblocked(Arg.Any<bool>(), Arg.Any<FriendActionSource>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendPlayerReport(Arg.Any<PlayerReportIssueType>(), Arg.Any<double>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendPlayerJoin(Arg.Any<FriendActionSource>())).Do(x => { });
            mockedSocialAnalytics.When(x => x.SendPlayEmote(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<EmoteSource>())).Do(x => { });

            return mockedSocialAnalytics;
        }
    }
}