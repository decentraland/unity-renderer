using DCL;
using System.Collections.Generic;
using static DCL.SettingsCommon.GeneralSettings;

namespace SocialFeaturesAnalytics
{
    public class SocialAnalytics : ISocialAnalytics
    {
        private const string PLAYER_MUTED = "user_muted";
        private const string PLAYER_UNMUTED = "user_unmuted";
        private const string VOICE_MESSAGE_STARTED_BY_FIRST_TIME = "voice_chat_start_recording";
        private const string VOICE_MESSAGE_SENT = "voice_message_sent";
        private const string VOICE_CHANNEL_CONNECTION = "voice_channel_connection";
        private const string VOICE_CHANNEL_DISCONNECTION = "voice_channel_disconnection";
        private const string VOICE_CHAT_PREFERENCES_CHANGED = "voice_chat_preferences_changed";
        private const string FRIEND_REQUEST_SENT = "friend_request_sent";
        private const string FRIEND_REQUEST_APPROVED = "friend_request_approved";
        private const string FRIEND_REQUEST_REJECTED = "friend_request_rejected";
        private const string FRIEND_REQUEST_CANCELLED = "friend_request_cancelled";
        private const string FRIEND_REQUEST_ERROR = "friend_request_error";
        private const string FRIEND_DELETED = "friend_deleted";
        private const string PASSPORT_OPENED = "passport_opened";
        private const string PASSPORT_CLOSED = "passport_closed";
        private const string PASSPORT_CLICKED_ON_COLLECTIONS = "passport_collections_click";
        private const string PASSPORT_STARTED_CONVERSATION = "passport_started_conversation";
        private const string PASSPORT_INSPECT_AVATAR = "passport_inspect_avatar";
        private const string PASSPORT_CLICK_LINK = "passport_clicked_link";
        private const string PASSPORT_WALLET_COPY = "passport_wallet_copy";
        private const string PASSPORT_USERNAME_COPY = "passport_username_copy";
        private const string PASSPORT_JUMP_IN = "passport_jump_in";
        private const string PASSPORT_EDIT_PROFILE = "passport_edit_profile";
        private const string PASSPORT_BUY_NFT = "passport_buy_nft";
        private const string PLAYER_BLOCKED = "user_blocked";
        private const string PLAYER_UNBLOCKED = "user_unblocked";
        private const string PLAYER_REPORT = "player_report";
        private const string PLAYER_JOIN = "player_join";
        private const string PLAY_EMOTE = "used_emote";
        private const string EMPTY_CHANNEL_CREATED = "chat_channel_created";
        private const string POPULATED_CHANNEL_JOINED = "player_joins_channel";
        private const string CHANNEL_LEAVE = "player_leaves_channel";
        private const string CHANNEL_SEARCH = "player_search_channel";
        private const string CHANNEL_LINK_CLICK = "player_clicks_channel_link";
        private const string MENTION_MESSAGE_SENT = "mention_message_sent";
        private const string MENTION_CLICKED = "mention_clicked";
        private const string MENTION_CREATED = "mention_created";

        public static SocialAnalytics i { get; private set; }

        private readonly IAnalytics analytics;
        private readonly IUserProfileBridge userProfileBridge;

        public SocialAnalytics(IAnalytics analytics, IUserProfileBridge userProfileBridge)
        {
            this.analytics = analytics;
            this.userProfileBridge = userProfileBridge;
            i ??= this;
        }

        public void SendPlayerMuted(string toUserId)
        {
            PlayerType? toPlayerType = GetPlayerTypeByUserId(toUserId);

            if (toPlayerType == null)
                return;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("to", toPlayerType.ToString());

            analytics.SendAnalytic(PLAYER_MUTED, data);
        }

        public void SendPlayerUnmuted(string toUserId)
        {
            PlayerType? toPlayerType = GetPlayerTypeByUserId(toUserId);

            if (toPlayerType == null)
                return;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("to", toPlayerType.ToString());

            analytics.SendAnalytic(PLAYER_UNMUTED, data);
        }

        public void SendVoiceMessageStartedByFirstTime() { analytics.SendAnalytic(VOICE_MESSAGE_STARTED_BY_FIRST_TIME, new Dictionary<string, string>()); }

        public void SendVoiceMessage(double messageLength, VoiceMessageSource source, string fromUserId)
        {
            PlayerType? fromPlayerType = GetPlayerTypeByUserId(fromUserId);

            if (fromPlayerType == null)
                return;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("from", fromPlayerType.ToString());
            data.Add("length", messageLength.ToString());
            data.Add("source", source.ToString());

            analytics.SendAnalytic(VOICE_MESSAGE_SENT, data);
        }

        public void SendVoiceChannelConnection(int numberOfPeers)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("numberOfPeers", numberOfPeers.ToString());

            analytics.SendAnalytic(VOICE_CHANNEL_CONNECTION, data);
        }

        public void SendVoiceChannelDisconnection() { analytics.SendAnalytic(VOICE_CHANNEL_DISCONNECTION, new Dictionary<string, string>()); }

        public void SendClickedOnCollectibles()
        {
            analytics.SendAnalytic(PASSPORT_CLICKED_ON_COLLECTIONS, new Dictionary<string, string>());
        }

        public void SendStartedConversation(PlayerActionSource source)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("source", source.ToString());

            analytics.SendAnalytic(PASSPORT_STARTED_CONVERSATION, data);
        }

        public void SendNftBuy(PlayerActionSource source)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("source", source.ToString());

            analytics.SendAnalytic(PASSPORT_BUY_NFT, data);
        }

        public void SendInspectAvatar(double timeSpent)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("timeSpent", timeSpent.ToString());

            analytics.SendAnalytic(PASSPORT_INSPECT_AVATAR, data);
        }

        public void SendLinkClick(PlayerActionSource source)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("source", source.ToString());

            analytics.SendAnalytic(PASSPORT_CLICK_LINK, data);
        }

        public void SendCopyWallet(PlayerActionSource source)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("source", source.ToString());

            analytics.SendAnalytic(PASSPORT_WALLET_COPY, data);
        }

        public void SendCopyUsername(PlayerActionSource source)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("source", source.ToString());

            analytics.SendAnalytic(PASSPORT_USERNAME_COPY, data);
        }

        public void SendJumpInToPlayer(PlayerActionSource source)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("source", source.ToString());

            analytics.SendAnalytic(PASSPORT_JUMP_IN, data);
        }

        public void SendProfileEdit(int descriptionLength, bool hasLinks, PlayerActionSource source)
        {
            var data = new Dictionary<string, string>
            {
                ["source"] = source.ToString(),
                ["descriptionLength"] = descriptionLength.ToString(),
                ["hasLinks"] = hasLinks.ToString(),
            };

            analytics.SendAnalytic(PASSPORT_EDIT_PROFILE, data);
        }

        public void SendVoiceChatPreferencesChanged(VoiceChatAllow preference)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("allow", preference.ToString());

            analytics.SendAnalytic(VOICE_CHAT_PREFERENCES_CHANGED, data);
        }

        public void SendFriendRequestError(string senderId, string recipientId, string source, string errorDescription)
        {
            var data = new Dictionary<string, string>
            {
                ["source"] = source,
                ["from"] = senderId,
                ["to"] = recipientId,
                ["description"] = errorDescription,
            };
            analytics.SendAnalytic(FRIEND_REQUEST_ERROR, data);
        }

        public void SendFriendRequestSent(string fromUserId, string toUserId, double messageLength, PlayerActionSource source)
        {
            PlayerType? fromPlayerType = GetPlayerTypeByUserId(fromUserId);
            PlayerType? toPlayerType = GetPlayerTypeByUserId(toUserId);

            if (fromPlayerType == null || toPlayerType == null)
                return;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("from", fromPlayerType.ToString());
            data.Add("to", toPlayerType.ToString());
            // TODO (FRIEND REQUESTS): retro-compatibility, remove this param in the future
            data.Add("text_length", messageLength.ToString());
            data.Add("attached_message_length", messageLength.ToString());
            data.Add("attached_message", messageLength > 0 ? "true" : "false");
            data.Add("source", source.ToString());

            analytics.SendAnalytic(FRIEND_REQUEST_SENT, data);
        }

        public void SendFriendRequestApproved(string fromUserId, string toUserId, string source, bool hasBodyMessage)
        {
            PlayerType? fromPlayerType = GetPlayerTypeByUserId(fromUserId);
            PlayerType? toPlayerType = GetPlayerTypeByUserId(toUserId);

            if (fromPlayerType == null || toPlayerType == null)
                return;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("from", fromPlayerType.ToString());
            data.Add("to", toPlayerType.ToString());
            data.Add("source", source);
            data.Add("has_body_message", hasBodyMessage ? "true" : "false");

            analytics.SendAnalytic(FRIEND_REQUEST_APPROVED, data);
        }

        public void SendFriendRequestRejected(string fromUserId, string toUserId, string source, bool hasBodyMessage)
        {
            PlayerType? fromPlayerType = GetPlayerTypeByUserId(fromUserId);
            PlayerType? toPlayerType = GetPlayerTypeByUserId(toUserId);

            if (fromPlayerType == null || toPlayerType == null)
                return;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("from", fromPlayerType.ToString());
            data.Add("to", toPlayerType.ToString());
            data.Add("source", source);
            data.Add("has_body_message", hasBodyMessage ? "true" : "false");

            analytics.SendAnalytic(FRIEND_REQUEST_REJECTED, data);
        }

        public void SendFriendRequestCancelled(string fromUserId, string toUserId, string source)
        {
            PlayerType? fromPlayerType = GetPlayerTypeByUserId(fromUserId);
            PlayerType? toPlayerType = GetPlayerTypeByUserId(toUserId);

            if (fromPlayerType == null || toPlayerType == null)
                return;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("from", fromPlayerType.ToString());
            data.Add("to", toPlayerType.ToString());
            data.Add("source", source);

            analytics.SendAnalytic(FRIEND_REQUEST_CANCELLED, data);
        }

        public void SendFriendDeleted(string fromUserId, string toUserId, PlayerActionSource source)
        {
            PlayerType? fromPlayerType = GetPlayerTypeByUserId(fromUserId);
            PlayerType? toPlayerType = GetPlayerTypeByUserId(toUserId);

            if (fromPlayerType == null || toPlayerType == null)
                return;

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("from", fromPlayerType.ToString());
            data.Add("to", toPlayerType.ToString());
            data.Add("source", source.ToString());

            analytics.SendAnalytic(FRIEND_DELETED, data);
        }

        public void SendMessageWithMention()
        {
            analytics.SendAnalytic(MENTION_MESSAGE_SENT, new Dictionary<string, string>());
        }

        public void SendClickedMention()
        {
            analytics.SendAnalytic(MENTION_CLICKED, new Dictionary<string, string>());
        }

        public void SendMentionCreated(MentionCreationSource source)
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                {"source", source.ToString()},
            };

            analytics.SendAnalytic(MENTION_CREATED, data);
        }

        public void SendPassportOpen(bool found = true, AvatarOpenSource source = AvatarOpenSource.World)
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                {"source", source.ToString()},
                {"found", found.ToString()}
            };

            analytics.SendAnalytic(PASSPORT_OPENED, data);
        }

        public void SendPassportClose(double timeSpent)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("time_spent", timeSpent.ToString());

            analytics.SendAnalytic(PASSPORT_CLOSED, data);
        }

        public void SendPlayerBlocked(bool isFriend, PlayerActionSource source)
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "friend", isFriend.ToString() },
                { "source", source.ToString() },
            };

            analytics.SendAnalytic(PLAYER_BLOCKED, data);
        }

        public void SendPlayerUnblocked(bool isFriend, PlayerActionSource source)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("friend", isFriend.ToString());
            data.Add("source", source.ToString());

            analytics.SendAnalytic(PLAYER_UNBLOCKED, data);
        }

        public void SendPlayerReport(PlayerReportIssueType issueType, double messageLength, PlayerActionSource source)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("issue_type", issueType.ToString());
            data.Add("text_length", messageLength.ToString());
            data.Add("source", source.ToString());

            analytics.SendAnalytic(PLAYER_REPORT, data);
        }

        public void SendPlayerJoin(PlayerActionSource source)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("source", source.ToString());

            analytics.SendAnalytic(PLAYER_JOIN, data);
        }

        public void SendPlayEmote(string emoteId, string emoteName, string rarity, bool isBaseEmote, UserProfile.EmoteSource source, string parcelLocation)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("id", emoteId);
            data.Add("name", emoteName);
            data.Add("rarity", rarity);
            data.Add("isBase", isBaseEmote.ToString());
            data.Add("source", source.ToString());
            data.Add("parcel_location", parcelLocation);

            analytics.SendAnalytic(PLAY_EMOTE, data);
        }

        public void SendEmptyChannelCreated(string channelName, ChannelJoinedSource source)
        {
            var data = new Dictionary<string, string>
            {
                ["source"] = source switch
                {
                    ChannelJoinedSource.Command => "command",
                    ChannelJoinedSource.Link => "link",
                    ChannelJoinedSource.Search => "create_search",
                    _ => ""
                },
                ["channel"] = channelName
            };
            analytics.SendAnalytic(EMPTY_CHANNEL_CREATED, data);
        }

        public void SendPopulatedChannelJoined(string channelName, ChannelJoinedSource source, string method)
        {
            var data = new Dictionary<string, string>
            {
                ["source"] = source switch
                {
                    ChannelJoinedSource.Command => "command",
                    ChannelJoinedSource.Link => "link",
                    ChannelJoinedSource.Search => "search",
                    ChannelJoinedSource.ConversationList => "conversation_list",
                    _ => ""
                },
                ["channel"] = channelName,
                ["method"] = method
            };
            analytics.SendAnalytic(POPULATED_CHANNEL_JOINED, data);
        }

        public void SendLeaveChannel(string channelId, ChannelLeaveSource source)
        {
            var data = new Dictionary<string, string>
            {
                ["source"] = source switch
                {
                    ChannelLeaveSource.Chat => "chat",
                    ChannelLeaveSource.Command => "command",
                    ChannelLeaveSource.Search => "search",
                    ChannelLeaveSource.ConversationList => "conversation_list",
                    _ => ""
                },
                ["channel"] = channelId
            };
            analytics.SendAnalytic(CHANNEL_LEAVE, data);
        }

        public void SendChannelSearch(string text)
        {
            var data = new Dictionary<string, string>
            {
                ["search"] = text
            };
            analytics.SendAnalytic(CHANNEL_SEARCH, data);
        }

        public void SendChannelLinkClicked(string channel, bool joinAccepted, ChannelLinkSource source)
        {
            var data = new Dictionary<string, string>
            {
                ["source"] = source switch
                {
                    ChannelLinkSource.Chat => "chat",
                    ChannelLinkSource.Event => "event",
                    ChannelLinkSource.Place => "place",
                    ChannelLinkSource.Profile => "profile",
                    _ => ""
                },
                ["channel"] = channel,
                ["result"] = joinAccepted ? "joined" : "cancel"
            };
            analytics.SendAnalytic(CHANNEL_LINK_CLICK, data);
        }

        private PlayerType? GetPlayerTypeByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return null;

            UserProfile userProfile = userProfileBridge.Get(userId);

            if (userProfile == null)
                return null;
            return userProfile.isGuest ? PlayerType.Guest : PlayerType.Wallet;
        }
    }
}
