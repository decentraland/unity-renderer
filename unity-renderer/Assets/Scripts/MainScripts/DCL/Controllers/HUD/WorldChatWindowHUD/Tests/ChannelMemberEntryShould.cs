using DCL.Chat.HUD;
using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;

public class ChannelMemberEntryShould
{
    private ChannelMemberEntry channelMemberEntryComponent;

    [SetUp]
    public void SetUp()
    {
        channelMemberEntryComponent = BaseComponentView.Create<ChannelMemberEntry>("SocialBarV1/ChannelMemberEntry");
        channelMemberEntryComponent.userThumbnail.imageObserver = Substitute.For<ILazyTextureObserver>();
    }

    [TearDown]
    public void TearDown()
    {
        channelMemberEntryComponent.Dispose();
    }

    [Test]
    public void SetUserIdCorrectly()
    {
        // Arrange
        string testUserId = "testId";

        // Act
        channelMemberEntryComponent.SetUserId(testUserId);

        // Assert
        Assert.AreEqual(testUserId, channelMemberEntryComponent.model.userId);
    }

    [Test]
    public void SetUserNameCorrectly()
    {
        // Arrange
        string testUserName = "testName";

        // Act
        channelMemberEntryComponent.SetUserName(testUserName);

        // Assert
        Assert.AreEqual(testUserName, channelMemberEntryComponent.model.userName);
        Assert.AreEqual(testUserName, channelMemberEntryComponent.nameLabel.text);
    }

    [Test]
    public void SetUserThumbnailCorrectly()
    {
        // Arrange
        string testUri = "testUri";

        // Act
        channelMemberEntryComponent.SetUserThumbnail(testUri);

        // Assert
        Assert.AreEqual(testUri, channelMemberEntryComponent.model.thumnailUrl);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetUserOnlineStatusCorrectly(bool isOnline)
    {
        // Arrange
        channelMemberEntryComponent.model.isOnline = !isOnline;

        // Act
        channelMemberEntryComponent.SetUserOnlineStatus(isOnline);

        // Assert
        Assert.AreEqual(isOnline, channelMemberEntryComponent.model.isOnline);
        Assert.AreEqual(isOnline, channelMemberEntryComponent.onlineMark.activeSelf);
        Assert.AreEqual(!isOnline, channelMemberEntryComponent.offlineMark.activeSelf);
    }
}
