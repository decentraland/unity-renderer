using Cysharp.Threading.Tasks;
using DCL.Chat;
using NSubstitute;
using NUnit.Framework;
using SocialFeaturesAnalytics;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Channel = DCL.Chat.Channels.Channel;

namespace DCL.Social.Chat.Channels
{
    public class JoinChannelComponentControllerShould
    {
        private JoinChannelComponentController joinChannelComponentController;
        private IJoinChannelComponentView view;
        private IChatController chatController;
        private DataStore_Channels channelsDataStore;
        private DataStore dataStore;
        private StringVariable currentPlayerInfoCardId;
        private ISocialAnalytics socialAnalytics;
        private IChannelsFeatureFlagService channelsFeatureFlagService;

        [SetUp]
        public void SetUp()
        {
            view = Substitute.For<IJoinChannelComponentView>();
            chatController = Substitute.For<IChatController>();
            dataStore = new DataStore();
            channelsDataStore = dataStore.channels;
            currentPlayerInfoCardId = ScriptableObject.CreateInstance<StringVariable>();
            socialAnalytics = Substitute.For<ISocialAnalytics>();
            channelsFeatureFlagService = Substitute.For<IChannelsFeatureFlagService>();
            channelsFeatureFlagService.IsChannelsFeatureEnabled().Returns(true);

            joinChannelComponentController = new JoinChannelComponentController(view, chatController,
                dataStore,
                socialAnalytics,
                currentPlayerInfoCardId,
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
            view.Received(string.IsNullOrEmpty(testChannelId) ? 0 : 1).SetChannel(testChannelId);
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
            const string TEST_CHANNEL_NAME = "TestId";
            const string CHANNEL_ID = "channelId";
            var channel = new Channel(CHANNEL_ID, TEST_CHANNEL_NAME, 0, 1, false, false, "");

            chatController.GetAllocatedChannelByName(TEST_CHANNEL_NAME.ToLower())
                          .Returns(channel);

            chatController.JoinOrCreateChannelAsync(CHANNEL_ID)
                          .Returns(UniTask.FromResult(channel));

            // Act
            view.OnConfirmJoin += Raise.Event<Action<string>>(TEST_CHANNEL_NAME);

            // Assert
            chatController.Received(1).JoinOrCreateChannelAsync(CHANNEL_ID);
            view.Received(1).Hide();
            Assert.IsNull(channelsDataStore.currentJoinChannelModal.Get());
        }

        [Test]
        public void JoinChannelWhenIsAlreadyCreatedButNotFetched()
        {
            // Arrange
            const string TEST_CHANNEL_NAME = "TestId";
            const string CHANNEL_ID = "channelId";

            chatController.GetAllocatedChannelByName(TEST_CHANNEL_NAME.ToLower()).Returns((Channel)null);

            var channel = new Channel(CHANNEL_ID, TEST_CHANNEL_NAME.ToLower(), 0, 1, false, false, "");

            chatController.GetChannelsByNameAsync(1, TEST_CHANNEL_NAME.ToLower(), null, Arg.Any<CancellationToken>())
                          .Returns(UniTask.FromResult(("", new[] { channel })));

            chatController.JoinOrCreateChannelAsync(CHANNEL_ID)
                          .Returns(UniTask.FromResult(channel));

            // Act
            view.OnConfirmJoin += Raise.Event<Action<string>>(TEST_CHANNEL_NAME);

            // Assert
            chatController.Received(1).JoinOrCreateChannelAsync(CHANNEL_ID);
            view.Received(1).Hide();
            Assert.IsNull(channelsDataStore.currentJoinChannelModal.Get());
        }

        [Test]
        public void DoNotJoinChannelWhenIsNotCreated()
        {
            const string TEST_CHANNEL_NAME = "TestId";
            const string CHANNEL_ID = "channelId";
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
            const string TEST_CHANNEL_NAME = "TestId";
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
        public void TrackChannelLinkClickWhenCancel()
        {
            const string channelId = "channelId";
            dataStore.HUDs.visibleTaskbarPanels.Set(new HashSet<string> { "PrivateChatChannel" });
            channelsDataStore.currentJoinChannelModal.Set(channelId, true);
            channelsDataStore.channelJoinedSource.Set(ChannelJoinedSource.Link);

            view.OnCancelJoin += Raise.Event<Action>();

            socialAnalytics.Received(1).SendChannelLinkClicked(channelId, false, ChannelLinkSource.Chat);
        }

        [Test]
        public void TrackChannelLinkClickWhenConfirm()
        {
            const string TEST_CHANNEL_NAME = "TestId";
            const string CHANNEL_ID = "channelId";
            var channel = new Channel(CHANNEL_ID, TEST_CHANNEL_NAME, 0, 1, false, false, "");

            chatController.GetAllocatedChannelByName(TEST_CHANNEL_NAME.ToLower())
                          .Returns(channel);

            chatController.JoinOrCreateChannelAsync(CHANNEL_ID)
                          .Returns(UniTask.FromResult(channel));

            currentPlayerInfoCardId.Set("userId");
            channelsDataStore.currentJoinChannelModal.Set(CHANNEL_ID, true);
            channelsDataStore.channelJoinedSource.Set(ChannelJoinedSource.Link);

            view.OnConfirmJoin += Raise.Event<Action<string>>(TEST_CHANNEL_NAME);

            socialAnalytics.Received(1).SendChannelLinkClicked(TEST_CHANNEL_NAME.ToLower(), true, ChannelLinkSource.Profile);
        }
    }
}
