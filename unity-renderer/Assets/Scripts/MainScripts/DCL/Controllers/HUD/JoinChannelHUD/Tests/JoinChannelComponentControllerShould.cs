using DCL;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;

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
        joinChannelComponentController = Substitute.ForPartsOf<JoinChannelComponentController>();
        joinChannelComponentController.Configure().CreateJoinChannelView().Returns(info => joinChannelComponentView);
        joinChannelComponentController.Initialize(chatController, channelsDataStore);
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
        joinChannelComponentController.OnChannelToJoinChanged(testChannelId, null);

        // Assert
        joinChannelComponentView.Received(string.IsNullOrEmpty(testChannelId) ? 0 : 1).SetChannel(testChannelId);
        joinChannelComponentView.Received(string.IsNullOrEmpty(testChannelId) ? 0 : 1).Show();
    }

    [Test]
    public void RaiseOnCancelJoinCorrectly()
    {
        // Act
        joinChannelComponentController.OnCancelJoin();

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
        joinChannelComponentController.OnConfirmJoin(testChannelId);

        // Assert
        chatController.Received(1).JoinOrCreateChannel(testChannelId);
        joinChannelComponentView.Received(1).Hide();
        Assert.IsNull(channelsDataStore.currentJoinChannelModal.Get());
    }
}
