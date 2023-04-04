using DCL.Chat.Notifications;
using NUnit.Framework;

public class ChatNotificationMessageComponentViewTest
{
    private ChatNotificationMessageComponentView chatNotificationComponent;
    private string testSpriteUri;

    [SetUp]
    public void SetUp()
    {
        chatNotificationComponent = BaseComponentView.Create<ChatNotificationMessageComponentView>("ChatNotificationMessage");
        testSpriteUri = "testuri";
    }

    [TearDown]
    public void TearDown()
    {
        chatNotificationComponent.Dispose();
    }

    [Test]
    public void SetOnClickCorrectly()
    {
        // Arrange
        bool isClicked = false;
        chatNotificationComponent.OnClickedNotification += (_) => isClicked = true;

        // Act
        chatNotificationComponent.button.onClick.Invoke();

        // Assert
        Assert.IsTrue(isClicked, "The button has not responded to the onClick action.");
    }

    [Test]
    public void ConfigurePublicNotificationCorrectly()
    {
        // Arrange
        ChatNotificationMessageComponentModel testModel = new ChatNotificationMessageComponentModel
        {
            message = "Test message",
            time = "12:44",
            messageHeader = "#general",
            messageSender = "SpottyGoat",
            isPrivate = false,
            imageUri = null,
            notificationTargetId = "testId",
            isImageVisible = false,
        };

        // Act
        chatNotificationComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, chatNotificationComponent.model, "The model does not match after configuring the notification.");
        Assert.False(chatNotificationComponent.image.gameObject.activeInHierarchy);
        Assert.False(chatNotificationComponent.imageContainer.activeInHierarchy);
    }

    [Test]
    public void ConfigurePrivateNotificationCorrectly()
    {
        // Arrange
        ChatNotificationMessageComponentModel testModel = new ChatNotificationMessageComponentModel
        {
            message = "Test message",
            time = "12:44",
            messageHeader = "PrivateChat",
            messageSender = "SpottyGoat",
            isPrivate = true,
            imageUri = null,
            notificationTargetId = "testId",
            isImageVisible = true
        };

        // Act
        chatNotificationComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, chatNotificationComponent.model, "The model does not match after configuring the notification.");
        Assert.True(chatNotificationComponent.image.gameObject.activeInHierarchy);
        Assert.True(chatNotificationComponent.imageContainer.activeInHierarchy);
    }

    [Test]
    public void SetHeaderTextCorrectly()
    {
        // Arrange
        string headerText = "TestHeader";

        // Act
        chatNotificationComponent.SetMaxHeaderCharacters(30);
        chatNotificationComponent.SetNotificationHeader(headerText);

        // Assert
        Assert.AreEqual(headerText, chatNotificationComponent.model.messageHeader, "The header text does not match in the model.");
        Assert.AreEqual(headerText, chatNotificationComponent.notificationHeader.text, "The header text does not match.");
    }

    [Test]
    public void SetHeaderTextAboveSizeCorrectly()
    {
        // Arrange
        string headerText = "TestHeader";
        string headerTruncatedText = "Test...";

        // Act
        chatNotificationComponent.SetMaxHeaderCharacters(4);
        chatNotificationComponent.SetNotificationHeader(headerText);

        // Assert
        Assert.AreEqual(headerText, chatNotificationComponent.model.messageHeader, "The header text does not match in the model.");
        Assert.AreEqual(headerTruncatedText, chatNotificationComponent.notificationHeader.text, "The header text does not match.");
    }

    [Test]
    public void SetMessageTextCorrectly()
    {
        // Arrange
        string messageText = "My test message";

        // Act
        chatNotificationComponent.SetMaxContentCharacters(50);
        chatNotificationComponent.SetMessage(messageText);

        // Assert
        Assert.AreEqual(messageText, chatNotificationComponent.model.message, "The message text does not match in the model.");
        Assert.AreEqual(messageText, chatNotificationComponent.notificationMessage.text, "The message text does not match.");
    }

    [Test]
    public void SetTimeCorrectly()
    {
        // Arrange
        string time = "11:33";

        // Act
        chatNotificationComponent.SetTimestamp(time);

        // Assert
        Assert.AreEqual(time, chatNotificationComponent.model.time, "The time text does not match in the model.");
        Assert.AreEqual(time, chatNotificationComponent.notificationTimestamp.text, "The time text does not match.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetPrivateCorrectly(bool isPrivate)
    {
        //Act
        chatNotificationComponent.SetIsPrivate(isPrivate);

        //Assert
        Assert.AreEqual(isPrivate, chatNotificationComponent.model.isPrivate, "The private status does not match in the model");
        Assert.AreEqual(isPrivate, chatNotificationComponent.isPrivate, "The private status does not match");
        Assert.AreEqual(isPrivate, chatNotificationComponent.image.gameObject.activeInHierarchy, "The icon active status does not reflect the isPrivate status");
    }
}
