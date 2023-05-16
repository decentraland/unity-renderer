using Cysharp.Threading.Tasks;
using DCL.Chat;
using DCL.Chat.Channels;
using NSubstitute;
using NUnit.Framework;
using SocialFeaturesAnalytics;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;
using Channel = DCL.Chat.Channels.Channel;

namespace DCL.Social.Chat.Channels
{
    public class JoinChannelComponentControllerShould
    {
        const string TEST_CHANNEL_NAME = "TestId";
        const string CHANNEL_ID = "channelId";
        private const string OPEN_PASSPORT_SOURCE = "ProfileHUD";

        private JoinChannelComponentController joinChannelComponentController;
        private IJoinChannelComponentView view;
        private IChatController chatController;
        private DataStore_Channels channelsDataStore;
        private DataStore dataStore;
        private BaseVariable<(string playerId, string source)> currentPlayerInfoCardId;
        private ISocialAnalytics socialAnalytics;
        private IChannelsFeatureFlagService channelsFeatureFlagService;

        [SetUp]
        public void SetUp()
        {
            view = Substitute.For<IJoinChannelComponentView>();
            chatController = Substitute.For<IChatController>();
            dataStore = new DataStore();
            channelsDataStore = dataStore.channels;
            currentPlayerInfoCardId = dataStore.HUDs.currentPlayerId;
            socialAnalytics = Substitute.For<ISocialAnalytics>();
            channelsFeatureFlagService = Substitute.For<IChannelsFeatureFlagService>();
            channelsFeatureFlagService.IsChannelsFeatureEnabled().Returns(true);

            joinChannelComponentController = new JoinChannelComponentController(view, chatController,
                dataStore,
                socialAnalytics,
                channelsFeatureFlagService);
        }

        [TearDown]
        public void TearDown()
        {
            joinChannelComponentController.Dispose();
        }

        [Test]
        public void InitializeCorrectly()
        {
            // Assert
            Assert.AreEqual(view, joinChannelComponentController.view);
            Assert.AreEqual(chatController, joinChannelComponentController.chatController);
            Assert.AreEqual(channelsDataStore, joinChannelComponentController.channelsDataStore);
        }

        [Test]
        [TestCase("TestId")]
        [TestCase(null)]
        public void RaiseOnChannelToJoinChangedCorrectly(string testChannelId)
        {
            // Act
            channelsDataStore.currentJoinChannelModal.Set(testChannelId, true);

            // Assert
            view.Received(string.IsNullOrEmpty(testChannelId) ? 0 : 1).SetChannel(testChannelId?.ToLower());
            view.Received(string.IsNullOrEmpty(testChannelId) ? 0 : 1).Show();
        }

        [Test]
        public void RaiseOnCancelJoinCorrectly()
        {
            // Act
            view.OnCancelJoin += Raise.Event<Action>();

            // Assert
            view.Received(1).Hide();
            Assert.IsNull(channelsDataStore.currentJoinChannelModal.Get());
        }

        [Test]
        public void JoinChannelWhenIsAlreadyAllocated()
        {
            // Arrange
            Channel channel = new (CHANNEL_ID, TEST_CHANNEL_NAME, 0, 1, false, false, "");

            chatController.GetAllocatedChannelByName(TEST_CHANNEL_NAME.ToLower())
                          .Returns(channel);

            chatController.JoinOrCreateChannelAsync(CHANNEL_ID, Arg.Any<CancellationToken>())
                          .Returns(UniTask.FromResult(channel));

            // Act
            view.OnConfirmJoin += Raise.Event<Action<string>>(TEST_CHANNEL_NAME);

            // Assert
            chatController.Received(1).JoinOrCreateChannelAsync(CHANNEL_ID, Arg.Any<CancellationToken>());
            view.Received(1).Hide();
            Assert.IsNull(channelsDataStore.currentJoinChannelModal.Get());
        }

        [Test]
        public void JoinChannelWhenIsAlreadyCreatedButNotFetched()
        {
            // Arrange
            chatController.GetAllocatedChannelByName(TEST_CHANNEL_NAME.ToLower()).Returns((Channel)null);

            Channel channel = new (CHANNEL_ID, TEST_CHANNEL_NAME.ToLower(), 0, 1, false, false, "");

            chatController.GetChannelsByNameAsync(1, TEST_CHANNEL_NAME.ToLower(), null, Arg.Any<CancellationToken>())
                          .Returns(UniTask.FromResult(("", new[] { channel })));

            chatController.JoinOrCreateChannelAsync(CHANNEL_ID, Arg.Any<CancellationToken>())
                          .Returns(UniTask.FromResult(channel));

            // Act
            view.OnConfirmJoin += Raise.Event<Action<string>>(TEST_CHANNEL_NAME);

            // Assert
            chatController.Received(1).JoinOrCreateChannelAsync(CHANNEL_ID, Arg.Any<CancellationToken>());
            view.Received(1).Hide();
            Assert.IsNull(channelsDataStore.currentJoinChannelModal.Get());
        }

        [Test]
        public void DoNotJoinChannelWhenIsNotCreated()
        {
            chatController.GetAllocatedChannelByName(TEST_CHANNEL_NAME.ToLower()).Returns((Channel)null);

            chatController.GetChannelsByNameAsync(1, TEST_CHANNEL_NAME.ToLower(), null, Arg.Any<CancellationToken>())
                          .Returns(UniTask.FromResult(("", new[] { new Channel(CHANNEL_ID, "TestIdWrong", 0, 1, false, false, "") })));

            // Act
            view.OnConfirmJoin += Raise.Event<Action<string>>(TEST_CHANNEL_NAME);

            // Assert
            chatController.Received(0).JoinOrCreateChannelAsync(Arg.Any<string>());
            view.Received(1).Hide();
        }

        [Test]
        public void DoNotJoinChannelWhenTheFetchedChannelIsNotTheSame()
        {
            chatController.GetAllocatedChannelByName(TEST_CHANNEL_NAME.ToLower()).Returns((Channel)null);

            chatController.GetChannelsByNameAsync(1, TEST_CHANNEL_NAME.ToLower(), null, Arg.Any<CancellationToken>())
                          .Returns(UniTask.FromResult(("", Array.Empty<Channel>())));

            // Act
            view.OnConfirmJoin += Raise.Event<Action<string>>(TEST_CHANNEL_NAME);

            // Assert
            chatController.Received(0).JoinOrCreateChannelAsync(Arg.Any<string>());
            view.Received(1).Hide();
        }

        [Test]
        public void HideWhenThereIsAnException()
        {
            LogAssert.Expect(LogType.Exception, new Regex("TimeoutException"));

            chatController.GetAllocatedChannelByName(TEST_CHANNEL_NAME.ToLower()).Returns((Channel)null);

            chatController.GetChannelsByNameAsync(1, TEST_CHANNEL_NAME.ToLower(), null, Arg.Any<CancellationToken>())
                          .Returns(UniTask.FromException<(string, Channel[])>(new TimeoutException()));

            view.OnConfirmJoin += Raise.Event<Action<string>>(TEST_CHANNEL_NAME);

            view.Received(1).Hide();
        }

        [Test]
        public void ShowErrorToastWhenThereIsAChannelException()
        {
            LogAssert.Expect(LogType.Exception, new Regex("ChannelException"));

            chatController.GetAllocatedChannelByName(TEST_CHANNEL_NAME.ToLower()).Returns((Channel)null);

            chatController.GetChannelsByNameAsync(1, TEST_CHANNEL_NAME.ToLower(), null, Arg.Any<CancellationToken>())
                          .Returns(UniTask.FromException<(string, Channel[])>(new ChannelException(CHANNEL_ID, ChannelErrorCode.Unknown)));

            var called = false;
            dataStore.notifications.DefaultErrorNotification.OnChange += (current, previous) => called = true;

            view.OnConfirmJoin += Raise.Event<Action<string>>(TEST_CHANNEL_NAME);

            Assert.IsTrue(called);
        }

        [Test]
        public void TrackChannelLinkClickWhenCancel()
        {
            dataStore.HUDs.visibleTaskbarPanels.Set(new HashSet<string> { "PrivateChatChannel" });
            channelsDataStore.currentJoinChannelModal.Set(TEST_CHANNEL_NAME, true);
            channelsDataStore.channelJoinedSource.Set(ChannelJoinedSource.Link);

            view.OnCancelJoin += Raise.Event<Action>();

            socialAnalytics.Received(1).SendChannelLinkClicked(TEST_CHANNEL_NAME.ToLower(), false, ChannelLinkSource.Chat);
        }

        [Test]
        public void TrackChannelLinkClickWhenConfirm()
        {
            Channel channel = new (CHANNEL_ID, TEST_CHANNEL_NAME, 0, 1, false, false, "");

            chatController.GetAllocatedChannelByName(TEST_CHANNEL_NAME.ToLower())
                          .Returns(channel);

            chatController.JoinOrCreateChannelAsync(CHANNEL_ID, Arg.Any<CancellationToken>())
                          .Returns(UniTask.FromResult(channel));

            currentPlayerInfoCardId.Set(("userId", OPEN_PASSPORT_SOURCE));
            channelsDataStore.currentJoinChannelModal.Set(CHANNEL_ID, true);
            channelsDataStore.channelJoinedSource.Set(ChannelJoinedSource.Link);

            view.OnConfirmJoin += Raise.Event<Action<string>>(TEST_CHANNEL_NAME);

            socialAnalytics.Received(1).SendChannelLinkClicked(TEST_CHANNEL_NAME.ToLower(), true, ChannelLinkSource.Profile);
        }

        [Test]
        public void OpenChannelWindowWhenShowAnAlreadyJoinedChannel()
        {
            var called = false;
            Channel channel = new (CHANNEL_ID, TEST_CHANNEL_NAME, 0, 1, true, false, "");

            chatController.GetAllocatedChannelByName(TEST_CHANNEL_NAME.ToLower())
                          .Returns(channel);

            channelsDataStore.channelToBeOpened.OnChange += (current, previous) => called = current == CHANNEL_ID;

            channelsDataStore.currentJoinChannelModal.Set(TEST_CHANNEL_NAME, true);

            view.Received(0).Show();
            Assert.IsTrue(called);
        }
    }
}
