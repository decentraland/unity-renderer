using DCL.Chat.HUD;
using NUnit.Framework;
using UnityEngine;

public class ChannelMemberEntryShould
{
    private ChannelMemberEntry view;

    [SetUp]
    public void SetUp()
    {
        view = ChannelMemberEntry.Create();
    }

    [TearDown]
    public void TearDown()
    {
        view.Dispose();
    }

    [Test]
    public void SetUserIdCorrectly()
    {
        // Arrange
        string testUserId = "testId";

        // Act
        view.SetUserId(testUserId);

        // Assert
        Assert.AreEqual(testUserId, view.model.userId);
    }

    [Test]
    public void SetUserNameCorrectly()
    {
        // Arrange
        string testUserName = "testName";

        // Act
        view.SetUserName(testUserName);

        // Assert
        Assert.AreEqual(testUserName, view.model.userName);
        Assert.AreEqual(testUserName, view.nameLabel.text);
    }

    [Test]
    public void SetUserThumbnailCorrectly()
    {
        // Arrange
        string testUri = "testUri";

        // Act
        view.SetUserThumbnail(testUri);

        // Assert
        Assert.AreEqual(testUri, view.model.thumnailUrl);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetUserOnlineStatusCorrectly(bool isOnline)
    {
        // Arrange
        view.model.isOnline = !isOnline;

        // Act
        view.SetUserOnlineStatus(isOnline);

        // Assert
        Assert.AreEqual(isOnline, view.model.isOnline);
        Assert.AreEqual(isOnline, view.onlineMark.activeSelf);
        Assert.AreEqual(!isOnline, view.offlineMark.activeSelf);
    }
}
