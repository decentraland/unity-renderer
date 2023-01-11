using DCL.Chat.Notifications;
using NUnit.Framework;

public class FriendRequestNotificationComponentViewTest
{
    private FriendRequestNotificationComponentView friendRequestNotificationComponent;

    [SetUp]
    public void SetUp()
    {
        friendRequestNotificationComponent = BaseComponentView.Create<FriendRequestNotificationComponentView>("FriendRequestNotification");
    }

    [TearDown]
    public void TearDown()
    {
        friendRequestNotificationComponent.Dispose();
    }

    [Test]
    public void SetOnClickCorrectly()
    {
        // Arrange
        bool isClicked = false;
        friendRequestNotificationComponent.OnClickedNotification += (_, _, _) => isClicked = true;

        // Act
        friendRequestNotificationComponent.button.onClick.Invoke();

        // Assert
        Assert.IsTrue(isClicked, "The button has not responded to the onClick action.");
    }

    [Test]
    public void ConfigureFrendRequestNotificationCorrectly()
    {
        // Arrange
        FriendRequestNotificationComponentModel testModel = new FriendRequestNotificationComponentModel
        {
            UserId = "testId",
            UserName = "SpottyGoat",
            Header = "Friend Request",
            Message = "Test message",
            Time = "12:44",
            IsAccepted = false
        };

        // Act
        friendRequestNotificationComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, friendRequestNotificationComponent.model, "The model does not match after configuring the notification.");
    }

    [Test]
    public void SetUserCorrectly()
    {
        // Arrange
        string testUserId = "testId";
        string testUserName = "Test User";

        // Act
        friendRequestNotificationComponent.SetUser(testUserId, testUserName);

        // Assert
        Assert.AreEqual(testUserId, friendRequestNotificationComponent.model.UserId);
        Assert.AreEqual(testUserName, friendRequestNotificationComponent.model.UserName);
        Assert.AreEqual(testUserName, friendRequestNotificationComponent.notificationSender.text);
    }

    [Test]
    public void SetMessageCorrectly()
    {
        // Arrange
        string testMessage = "Test message";

        // Act
        friendRequestNotificationComponent.SetMessage(testMessage);

        // Assert
        Assert.AreEqual(testMessage, friendRequestNotificationComponent.model.Message);
        Assert.AreEqual(testMessage, friendRequestNotificationComponent.notificationMessage.text);
    }

    [Test]
    public void SetTimestampCorrectly()
    {
        // Arrange
        string testTimestamp = "12:43";

        // Act
        friendRequestNotificationComponent.SetTimestamp(testTimestamp);

        // Assert
        Assert.AreEqual(testTimestamp, friendRequestNotificationComponent.model.Time);
        Assert.AreEqual(testTimestamp, friendRequestNotificationComponent.notificationTimestamp.text);
    }

    [Test]
    public void SetHeaderCorrectly()
    {
        // Arrange
        string testHeader = "Test header";

        // Act
        friendRequestNotificationComponent.SetHeader(testHeader);

        // Assert
        Assert.AreEqual(testHeader, friendRequestNotificationComponent.model.Header);
        Assert.AreEqual(testHeader, friendRequestNotificationComponent.notificationHeader.text);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetIsAcceptedCorrectly(bool TestIsAccepted)
    {
        // Act
        friendRequestNotificationComponent.SetIsAccepted(TestIsAccepted);

        // Assert
        Assert.AreEqual(TestIsAccepted, friendRequestNotificationComponent.model.IsAccepted);
    }
}
