using Cysharp.Threading.Tasks;
using DCL.Interface;
using DCL.Social.Chat.Mentions;
using DCL.Social.Friends;
using NSubstitute;
using NUnit.Framework;
using SocialFeaturesAnalytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace DCL.Social.Chat
{
    public class PrivateChatWindowControllerShould
    {
        private const string BLOCKED_FRIEND_ID = "blocked-friend-id";
        private const string FRIEND_ID = "my-user-id-2";
        private const string FRIEND_NAME = "myFriendName";
        private const string OWN_USER_ID = "my-user-id";

        private PrivateChatWindowController controller;
        private IPrivateChatComponentView view;
        private IChatHUDComponentView internalChatView;
        private IUserProfileBridge userProfileBridge;
        private ISocialAnalytics socialAnalytics;
        private IChatController chatController;
        private IFriendsController friendsController;
        private IMouseCatcher mouseCatcher;
        private DataStore dataStore;
        private IChatMentionSuggestionProvider mentionSuggestionProvider;

        [SetUp]
        public void SetUp()
        {
            view = Substitute.For<IPrivateChatComponentView>();
            internalChatView = Substitute.For<IChatHUDComponentView>();
            socialAnalytics = Substitute.For<ISocialAnalytics>();
            view.ChatHUD.Returns(internalChatView);

            userProfileBridge = Substitute.For<IUserProfileBridge>();
            friendsController = Substitute.For<IFriendsController>();

            GivenOwnProfile();
            GivenFriend(FRIEND_ID, FRIEND_NAME, PresenceStatus.ONLINE);
            GivenFriend(BLOCKED_FRIEND_ID, "blockedFriendName", PresenceStatus.OFFLINE);

            chatController = Substitute.For<IChatController>();

            mouseCatcher = Substitute.For<IMouseCatcher>();
            dataStore = new DataStore();
            dataStore.featureFlags.flags.Set(new FeatureFlag { flags = { ["chat_mentions_enabled"] = true } });
            mentionSuggestionProvider = Substitute.For<IChatMentionSuggestionProvider>();

            controller = new PrivateChatWindowController(
                dataStore,
                userProfileBridge,
                chatController,
                friendsController,
                socialAnalytics,
                mouseCatcher,
                mentionSuggestionProvider,
                Substitute.For<IClipboard>());
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
        }

        [Test]
        public void ClearAllMessagesWhenInitialize()
        {
            WhenControllerInitializes(FRIEND_ID);

            internalChatView.Received(1).ClearAllEntries();
        }

        [Test]
        public void ReceivesOneMessageProperly()
        {
            WhenControllerInitializes(FRIEND_ID);

            var msg1 = new ChatMessage(ChatMessage.Type.PRIVATE, FRIEND_ID, "message1");
            var msg2 = new ChatMessage(ChatMessage.Type.PRIVATE, FRIEND_ID, "message2");
            var msg3 = new ChatMessage(ChatMessage.Type.PRIVATE, FRIEND_ID, "message3");

            chatController.OnAddMessage += Raise.Event<Action<ChatMessage[]>>(new[] { msg1 });
            chatController.OnAddMessage += Raise.Event<Action<ChatMessage[]>>(new[] { msg2 });
            chatController.OnAddMessage += Raise.Event<Action<ChatMessage[]>>(new[] { msg3 });

            internalChatView.Received(3)
                            .SetEntry(Arg.Is<ChatEntryModel>(model =>
                                 model.messageType == ChatMessage.Type.PRIVATE
                                 && model.senderId == FRIEND_ID));
        }

        [Test]
        public void SendChatMessageWhenViewTriggers()
        {
            WhenControllerInitializes(FRIEND_ID);

            internalChatView.OnSendMessage += Raise.Event<Action<ChatMessage>>(new ChatMessage { body = "test message" });

            chatController.Received(1)
                          .Send(Arg.Is<ChatMessage>(message =>
                               message.body == $"/w {FRIEND_NAME} test message"
                               && message.recipient == FRIEND_NAME));
        }

        [Test]
        public void CloseWhenCloseButtonIsPressed()
        {
            var isViewActive = false;
            view.When(v => v.Show()).Do(info => isViewActive = true);
            view.When(v => v.Hide()).Do(info => isViewActive = false);
            view.IsActive.Returns(info => isViewActive);

            WhenControllerInitializes(FRIEND_ID);
            controller.SetVisibility(true);
            Assert.IsTrue(isViewActive);

            view.OnClose += Raise.Event<Action>();
            Assert.IsFalse(isViewActive);
        }

        [Test]
        public void TriggerBackWhenViewPressesBack()
        {
            WhenControllerInitializes(FRIEND_ID);

            var eventCalled = false;
            controller.OnBack += () => eventCalled = true;

            controller.SetVisibility(true);
            view.OnPressBack += Raise.Event<Action>();

            Assert.AreEqual(true, eventCalled);
        }

        [Test]
        public void SetupViewCorrectly()
        {
            WhenControllerInitializes(FRIEND_ID);
            controller.SetVisibility(true);

            view.Received(1).Setup(Arg.Is<UserProfile>(u => u.userId == FRIEND_ID), true, false);
        }

        [Test]
        public void SetUpViewAsBlocked()
        {
            WhenControllerInitializes(BLOCKED_FRIEND_ID);
            controller.SetVisibility(true);

            view.Received(1).Setup(Arg.Is<UserProfile>(u => u.userId == BLOCKED_FRIEND_ID), false, true);
        }

        [Test]
        public void AvoidReloadingChatsWhenIsTheSameUser()
        {
            WhenControllerInitializes(FRIEND_ID);
            controller.SetVisibility(true);
            controller.SetVisibility(false);
            WhenControllerInitializes(FRIEND_ID);
            controller.SetVisibility(true);

            chatController.ReceivedWithAnyArgs(1).GetPrivateMessages(default, default, default);
        }

        [Test]
        public void Show()
        {
            var isViewActive = false;
            view.When(v => v.Show()).Do(info => isViewActive = true);
            view.When(v => v.Hide()).Do(info => isViewActive = false);
            view.IsActive.Returns(info => isViewActive);

            WhenControllerInitializes(FRIEND_ID);
            controller.SetVisibility(true);

            internalChatView.Received(1).FocusInputField();
            view.Received().Setup(Arg.Is<UserProfile>(u => u.userId == FRIEND_ID), true, false);
            view.Received(1).Show();
            Assert.IsTrue(isViewActive);
            chatController.Received(1).MarkMessagesAsSeen(FRIEND_ID);
        }

        [Test]
        public void Hide()
        {
            var isViewActive = false;
            view.When(v => v.Show()).Do(info => isViewActive = true);
            view.When(v => v.Hide()).Do(info => isViewActive = false);
            view.IsActive.Returns(info => isViewActive);

            WhenControllerInitializes(FRIEND_ID);
            controller.SetVisibility(true);
            controller.SetVisibility(false);

            internalChatView.Received(1).UnfocusInputField();
            view.Received(1).Hide();
            Assert.IsFalse(isViewActive);
        }

        [Test]
        public void HideViewWhenMouseIsLocked()
        {
            WhenControllerInitializes(FRIEND_ID);
            controller.SetVisibility(true);
            view.IsActive.Returns(true);

            mouseCatcher.OnMouseLock += Raise.Event<Action>();

            view.Received(1).Hide();
            internalChatView.Received(1).UnfocusInputField();
        }

        [Test]
        public void ActivatePanel()
        {
            WhenControllerInitializes(FRIEND_ID);
            controller.SetVisibility(true);

            view.Received(1).Show();
        }

        [Test]
        public void RequestPrivateMessagesCorrectly()
        {
            controller.Initialize(view);
            string userId = "testId";
            int limit = 30;
            string testMessageId = "testId";

            controller.RequestPrivateMessages(userId, limit, testMessageId);

            view.Received(1).SetLoadingMessagesActive(true);
            chatController.Received(1).GetPrivateMessages(userId, limit, testMessageId);
        }

        [Test]
        public void RequestOldConversationsCorrectly()
        {
            WhenControllerInitializes(FRIEND_ID);

            controller.RequestOldConversations();

            view.Received(1).SetOldMessagesLoadingActive(true);

            chatController.Received(1)
                          .GetPrivateMessages(
                               FRIEND_ID,
                               PrivateChatWindowController.USER_PRIVATE_MESSAGES_TO_REQUEST_FOR_SHOW_MORE,
                               null);
        }

        [Test]
        public void MarkAsSeenOnlyOnceWhenManyMessagesReceived()
        {
            WhenControllerInitializes(FRIEND_ID);
            chatController.ClearReceivedCalls();
            view.IsActive.Returns(true);

            chatController.OnAddMessage += Raise.Event<Action<ChatMessage[]>>(new[]
            {
                new ChatMessage(
                    "msg1", ChatMessage.Type.PRIVATE, FRIEND_ID, "hey", 100)
                {
                    recipient = OWN_USER_ID
                },
                new ChatMessage(
                    "msg2", ChatMessage.Type.PRIVATE, FRIEND_ID, "hey", 101)
                {
                    recipient = OWN_USER_ID
                }
            });

            chatController.Received(1).MarkMessagesAsSeen(FRIEND_ID);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void CheckOwnPlayerMentionInDMsCorrectly(bool ownPlayerIsMentioned)
        {
            WhenControllerInitializes(FRIEND_ID);
            dataStore.mentions.ownPlayerMentionedInDM.Set(null, false);

            string testMessage = ownPlayerIsMentioned
                ? $"Hi <link=mention://{userProfileBridge.GetOwn().userName}><color=#4886E3><u>@{userProfileBridge.GetOwn().userName}</u></color></link>"
                : "test message";

            view.IsActive.Returns(false);

            var testMentionMessage = new ChatMessage
            {
                messageType = ChatMessage.Type.PRIVATE,
                body = testMessage,
                sender = FRIEND_ID,
                timestamp = 100,
            };

            chatController.OnAddMessage += Raise.Event<Action<ChatMessage[]>>(new[] { testMentionMessage });

            Assert.AreEqual(ownPlayerIsMentioned ? FRIEND_ID : null, dataStore.mentions.ownPlayerMentionedInDM.Get());
        }

        [TestCase("@", "")]
        [TestCase("hey @f", "f")]
        [TestCase("im super @dude", "dude")]
        public void SuggestNearbyUsers(string text, string name)
        {
            WhenControllerInitializes(FRIEND_ID);
            mentionSuggestionProvider.GetProfilesStartingWith(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<IEnumerable<UserProfile>>(), Arg.Any<CancellationToken>())
                                     .Returns(UniTask.FromResult(new List<UserProfile>()));

            internalChatView.OnMessageUpdated += Raise.Event<Action<string, int>>(text, 1);

            mentionSuggestionProvider.Received(1)
                                     .GetProfilesStartingWith(name, 5, Arg.Is<IEnumerable<UserProfile>>(profiles =>
                                          profiles.First().userId == OWN_USER_ID
                                          && profiles.Last().userId == FRIEND_ID
                                          && profiles.Count() == 2), Arg.Any<CancellationToken>());
        }

        private void WhenControllerInitializes(string friendId)
        {
            controller.Initialize(view);
            controller.Setup(friendId);
        }

        private void GivenOwnProfile()
        {
            var ownProfileModel = new UserProfileModel
            {
                userId = OWN_USER_ID,
                name = "NO_USER",
                blocked = new List<string> { BLOCKED_FRIEND_ID }
            };

            var ownUserProfile = ScriptableObject.CreateInstance<UserProfile>();
            ownUserProfile.UpdateData(ownProfileModel);

            userProfileBridge = Substitute.For<IUserProfileBridge>();
            userProfileBridge.GetOwn().Returns(ownUserProfile);
            userProfileBridge.Get(ownProfileModel.userId).Returns(ownUserProfile);
        }

        private void GivenFriend(string friendId, string name, PresenceStatus presence)
        {
            var testUserProfile = ScriptableObject.CreateInstance<UserProfile>();

            testUserProfile.UpdateData(new UserProfileModel
            {
                userId = friendId,
                name = name
            });

            userProfileBridge.Get(friendId).Returns(testUserProfile);

            friendsController.GetUserStatus(testUserProfile.userId)
                             .Returns(new UserStatus
                              {
                                  presence = presence,
                                  friendshipStatus = FriendshipStatus.FRIEND,
                              });
        }
    }
}
