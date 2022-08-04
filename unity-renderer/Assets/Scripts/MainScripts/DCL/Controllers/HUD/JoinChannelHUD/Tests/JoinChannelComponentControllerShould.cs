using DCL;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System;

public class JoinChannelComponentControllerShould
{
    private JoinChannelComponentController joinChannelComponentController;
    private IJoinChannelComponentView joinChannelComponentView;
    private IChatController chatController;
    private DataStore_Channels channelsDataStore;

    [SetUp]
    public void SetUp()
    {
        joinChannelComponentView = Substitute.For<IJoinChannelComponentView>();
        chatController = Substitute.For<IChatController>();
        channelsDataStore = new DataStore_Channels();
        joinChannelComponentController = new JoinChannelComponentController(joinChannelComponentView, chatController, channelsDataStore);
    }

    [TearDown]
    public void TearDown() { joinChannelComponentController.Dispose(); }

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
        chatController.Received(1).JoinOrCreateChannel(testChannelId);
        joinChannelComponentView.Received(1).Hide();
        Assert.IsNull(channelsDataStore.currentJoinChannelModal.Get());
    }
}
