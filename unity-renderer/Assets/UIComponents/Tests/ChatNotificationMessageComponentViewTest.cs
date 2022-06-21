using NUnit.Framework;
using UnityEngine;

public class ChatNotificationMessageComponentViewTest
{
    private ChatNotificationMessageComponentView chatNotificationComponent;
    private Texture2D testTexture;
    private Sprite testSprite;

    [SetUp]
    public void SetUp()
    {
        chatNotificationComponent = BaseComponentView.Create<ChatNotificationMessageComponentView>("ChatNotificationMessage");
        testTexture = new Texture2D(20, 20);
        testSprite = Sprite.Create(testTexture, new Rect(), Vector2.zero);
    }

    [TearDown]
    public void TearDown()
    {
        chatNotificationComponent.Dispose();
        GameObject.Destroy(chatNotificationComponent.gameObject);
        GameObject.Destroy(testTexture);
        GameObject.Destroy(testSprite);
    }

    [Test]
    public void SetOnClickCorrectly()
    {
        // Arrange
        bool isClicked = false;
        chatNotificationComponent.onClick.AddListener(() => isClicked = true);

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
            isPrivate = false,
            profileIcon = null,
            notificationTargetId = "testId"
        };

        // Act
        chatNotificationComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, chatNotificationComponent.model, "The model does not match after configuring the notification.");
        Assert.False(chatNotificationComponent.icon.gameObject.activeInHierarchy);
    }

    [Test]
    public void ConfigurePrivateNotificationCorrectly()
    {
        // Arrange
        ChatNotificationMessageComponentModel testModel = new ChatNotificationMessageComponentModel
        {
            message = "Test message",
            time = "12:44",
            messageHeader = "SpottyGoat",
            isPrivate = true,
            profileIcon = testSprite,
            notificationTargetId = "testId"
        };

        // Act
        chatNotificationComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, chatNotificationComponent.model, "The model does not match after configuring the notification.");
        Assert.True(chatNotificationComponent.icon.gameObject.activeInHierarchy);
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
    public void SetMessageTextAboveSizeCorrectly()
    {
        // Arrange
        string messageText = "My test message";
        string truncatedMessageText = "My t...";

        // Act
        chatNotificationComponent.SetMaxContentCharacters(4);
        chatNotificationComponent.SetMessage(messageText);

        // Assert
        Assert.AreEqual(messageText, chatNotificationComponent.model.message, "The message text does not match in the model.");
        Assert.AreEqual(truncatedMessageText, chatNotificationComponent.notificationMessage.text, "The message text does not match.");
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
        Assert.AreEqual(isPrivate, chatNotificationComponent.icon.gameObject.activeInHierarchy, "The icon active status does not reflect the isPrivate status");
    }
}
