using Cysharp.Threading.Tasks;
using DCL.Interface;
using DCL.ProfanityFiltering;
using DCL.Social.Chat.Mentions;
using NSubstitute;
using NUnit.Framework;
using SocialFeaturesAnalytics;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Channel = DCL.Chat.Channels.Channel;

namespace DCL.Social.Chat
{
    public class ChatChannelHUDControllerShould
    {
        private const string CHANNEL_ID = "channelId";
        private const string CHANNEL_NAME = "channelName";

        private ChatChannelHUDController controller;
        private IChatChannelWindowView view;
        private IChatHUDComponentView chatView;
        private IChatController chatController;
        private DataStore dataStore;
        private ISocialAnalytics socialAnalytics;
        private IProfanityFilter profanityFilter;
        private IUserProfileBridge userProfileBridge;
        private IChatMentionSuggestionProvider mentionSuggestionProvider;
        private IClipboard clipboard;

        [SetUp]
        public void SetUp()
        {
            userProfileBridge = Substitute.For<IUserProfileBridge>();
            var ownUserProfile = ScriptableObject.CreateInstance<UserProfile>();
            ownUserProfile.UpdateData(new UserProfileModel
            {
                userId = "ownUserId",
                name = "self"
            });
            userProfileBridge.GetOwn().Returns(ownUserProfile);

            chatController = Substitute.For<IChatController>();
            chatController.GetAllocatedChannel(CHANNEL_ID)
                .Returns(new Channel(CHANNEL_ID, CHANNEL_NAME, 4, 12, true, false, "desc"));

            dataStore = new DataStore();
            dataStore.featureFlags.flags.Set(new FeatureFlag { flags = { ["chat_mentions_enabled"] = true } });
            socialAnalytics = Substitute.For<ISocialAnalytics>();
            profanityFilter = Substitute.For<IProfanityFilter>();
            mentionSuggestionProvider = Substitute.For<IChatMentionSuggestionProvider>();

            clipboard = Substitute.For<IClipboard>();

            controller = new ChatChannelHUDController(dataStore,
                userProfileBridge,
                chatController,
                Substitute.For<IMouseCatcher>(),
                socialAnalytics,
                profanityFilter,
                mentionSuggestionProvider,
                clipboard);

            view = Substitute.For<IChatChannelWindowView>();
            chatView = Substitute.For<IChatHUDComponentView>();
            view.ChatHUD.Returns(chatView);

            controller.Initialize(view, false);
            controller.Setup(CHANNEL_ID);
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
        }

        [Test]
        public void LeaveChannelViaChatCommand()
        {
            chatView.OnSendMessage += Raise.Event<Action<ChatMessage>>(new ChatMessage
            {
                body = "/leave",
                messageType = ChatMessage.Type.PUBLIC
            });

            Assert.AreEqual(ChannelLeaveSource.Command, dataStore.channels.channelLeaveSource.Get());
            chatController.Received(1).LeaveChannel(CHANNEL_ID);
        }

        [Test]
        public void GoBackWhenLeavingChannel()
        {
            var backCalled = false;
            controller.OnPressBack += () => backCalled = true;
            controller.SetVisibility(true);

            chatController.OnChannelLeft += Raise.Event<Action<string>>(CHANNEL_ID);

            Assert.IsTrue(backCalled);
        }

        [Test]
        public void LeaveChannelWhenViewRequests()
        {
            string channelToLeave = "";
            controller.OnOpenChannelLeave += channelId =>
            {
                channelToLeave = channelId;
            };
            view.OnLeaveChannel += Raise.Event<Action>();

            Assert.AreEqual(ChannelLeaveSource.Chat, dataStore.channels.channelLeaveSource.Get());
            Assert.AreEqual(channelToLeave, CHANNEL_ID);
        }

        [Test]
        public void MuteChannel()
        {
            view.OnMuteChanged += Raise.Event<Action<bool>>(true);

            chatController.Received(1).MuteChannel(CHANNEL_ID);
        }

        [Test]
        public void UnmuteChannel()
        {
            view.OnMuteChanged += Raise.Event<Action<bool>>(false);

            chatController.Received(1).UnmuteChannel(CHANNEL_ID);
        }

        [Test]
        public void MarkMessagesAsSeenOnlyOnceWhenReceivedManyMessages()
        {
            var senderUserProfile = ScriptableObject.CreateInstance<UserProfile>();
            senderUserProfile.UpdateData(new UserProfileModel
            {
                userId = "user",
                name = "userName",
            });

            userProfileBridge.Get("user").Returns(senderUserProfile);

            controller.SetVisibility(true);
            view.IsActive.Returns(true);
            chatController.ClearReceivedCalls();

            var msg1 = new ChatMessage("msg1", ChatMessage.Type.PUBLIC, "user", "hey", 100)
            {
                recipient = CHANNEL_ID
            };
            var msg2 = new ChatMessage("msg1", ChatMessage.Type.PUBLIC, "user", "hey", 100)
            {
                recipient = CHANNEL_ID
            };

            chatController.OnAddMessage += Raise.Event<Action<ChatMessage[]>>(new[] {msg1, msg2});

            chatController.Received(1).MarkChannelMessagesAsSeen(CHANNEL_ID);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void CheckOwnPlayerMentionInChannelsCorrectly(bool ownPlayerIsMentioned)
        {
            controller.SetVisibility(false);
            dataStore.mentions.ownPlayerMentionedInChannel.Set(null, false);
            string testMessage = ownPlayerIsMentioned
                ? $"Hi <link=mention://{userProfileBridge.GetOwn().userName}><color=#4886E3><u>@{userProfileBridge.GetOwn().userName}</u></color></link>"
                : "test message";
            view.IsActive.Returns(false);

            var testMentionMessage = new ChatMessage
            {
                messageType = ChatMessage.Type.PUBLIC,
                body = testMessage,
                recipient = CHANNEL_ID,
                timestamp = 100,
            };
            chatController.OnAddMessage += Raise.Event<Action<ChatMessage[]>>(new[] {testMentionMessage});

            Assert.AreEqual(ownPlayerIsMentioned ? CHANNEL_ID : null, dataStore.mentions.ownPlayerMentionedInChannel.Get());
        }

        [TestCase("@", "")]
        [TestCase("hey @f", "f")]
        [TestCase("im super @dude", "dude")]
        public void SuggestNearbyUsers(string text, string name)
        {
            mentionSuggestionProvider.GetProfilesFromChatChannelsStartingWith(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                                     .Returns(UniTask.FromResult(new List<UserProfile>()));

            chatView.OnMessageUpdated += Raise.Event<Action<string, int>>(text, 1);

            mentionSuggestionProvider.Received(1).GetProfilesFromChatChannelsStartingWith(name, CHANNEL_ID, 5, Arg.Any<CancellationToken>());
        }

        [Test]
        public void CopyChannelNameToClipboard()
        {
            view.OnCopyNameRequested += Raise.Event<Action<string>>("#my-channel");

            clipboard.Received(1).WriteText("#my-channel");
        }
    }
}
