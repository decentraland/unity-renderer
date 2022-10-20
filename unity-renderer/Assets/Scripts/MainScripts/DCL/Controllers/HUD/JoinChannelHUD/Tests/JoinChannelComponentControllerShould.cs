using DCL;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using SocialFeaturesAnalytics;
using UnityEngine;
using DCL.Chat;

public class JoinChannelComponentControllerShould
{
    private JoinChannelComponentController joinChannelComponentController;
    private IJoinChannelComponentView joinChannelComponentView;
    private IChatController chatController;
    private DataStore_Channels channelsDataStore;
    private DataStore dataStore;
    private StringVariable currentPlayerInfoCardId;
    private ISocialAnalytics socialAnalytics;
    private IChannelsFeatureFlagService channelsFeatureFlagService;

    [SetUp]
    public void SetUp()
    {
        joinChannelComponentView = Substitute.For<IJoinChannelComponentView>();
        chatController = Substitute.For<IChatController>();
        dataStore = new DataStore();
        channelsDataStore = dataStore.channels;
        currentPlayerInfoCardId = ScriptableObject.CreateInstance<StringVariable>();
        socialAnalytics = Substitute.For<ISocialAnalytics>();
        channelsFeatureFlagService = Substitute.For<IChannelsFeatureFlagService>();
        channelsFeatureFlagService.IsChannelsFeatureEnabled().Returns(true);
        joinChannelComponentController = new JoinChannelComponentController(joinChannelComponentView, chatController,
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
        Assert.AreEqual(joinChannelComponentView, joinChannelComponentController.joinChannelView);
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
        joinChannelComponentView.Received(string.IsNullOrEmpty(testChannelId) ? 0 : 1).SetChannel(testChannelId);
        joinChannelComponentView.Received(string.IsNullOrEmpty(testChannelId) ? 0 : 1).Show();
    }

    [Test]
    public void RaiseOnCancelJoinCorrectly()
    {
        // Act
        joinChannelComponentView.OnCancelJoin += Raise.Event<Action>();

        // Assert
        joinChannelComponentView.Received(1).Hide();
        Assert.IsNull(channelsDataStore.currentJoinChannelModal.Get());
    }

    [Test]
    public void RaiseOnConfirmJoinCorrectly()
    {
        // Arrange
        string testChannelId = "TestId";

        // Act
        joinChannelComponentView.OnConfirmJoin += Raise.Event<Action<string>>(testChannelId);

        // Assert
        chatController.Received(1).JoinOrCreateChannel(testChannelId.ToLower());
        joinChannelComponentView.Received(1).Hide();
        Assert.IsNull(channelsDataStore.currentJoinChannelModal.Get());
    }

    [Test]
    public void TrackChannelLinkClickWhenCancel()
    {
        const string channelId = "channelId";
        dataStore.HUDs.visibleTaskbarPanels.Set(new HashSet<string> {"PrivateChatChannel"});
        channelsDataStore.currentJoinChannelModal.Set(channelId, true);
        channelsDataStore.channelJoinedSource.Set(ChannelJoinedSource.Link);
        
        joinChannelComponentView.OnCancelJoin += Raise.Event<Action>();

        socialAnalytics.Received(1).SendChannelLinkClicked(channelId, false, ChannelLinkSource.Chat);
    }
    
    [Test]
    public void TrackChannelLinkClickWhenConfirm()
    {
        const string channelId = "channelId";
        currentPlayerInfoCardId.Set("userId");
        channelsDataStore.currentJoinChannelModal.Set(channelId, true);
        channelsDataStore.channelJoinedSource.Set(ChannelJoinedSource.Link);
        
        joinChannelComponentView.OnConfirmJoin += Raise.Event<Action<string>>(channelId);

        socialAnalytics.Received(1).SendChannelLinkClicked(channelId.ToLower(), true, ChannelLinkSource.Profile);
    }
}