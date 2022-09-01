using NUnit.Framework;

public class VoiceChatBarComponentViewShould
{
    private VoiceChatBarComponentView voiceChatBarComponent;

    [SetUp]
    public void SetUp()
    {
        voiceChatBarComponent = BaseComponentView.Create<VoiceChatBarComponentView>("SocialBarV1/VoiceChatBar");
    }

    [TearDown]
    public void TearDown()
    {
        voiceChatBarComponent.Dispose();
    }

    [Test]
    public void ConfigureCorrectly()
    {
        // Arrange
        VoiceChatBarComponentModel testModel = new VoiceChatBarComponentModel
        {
            isSomeoneTalking = false,
            message = "Test message"
        };

        // Act
        voiceChatBarComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, voiceChatBarComponent.model);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetTalkingMessageCorrectly(bool isSomeoneTalking)
    {
        // Arrange
        string testMessage = "Test message";
        voiceChatBarComponent.model.message = "";
        voiceChatBarComponent.someoneTalkingContainer.gameObject.SetActive(!isSomeoneTalking);
        voiceChatBarComponent.altText.gameObject.SetActive(isSomeoneTalking);
        voiceChatBarComponent.someoneTalkingText.text = "";
        voiceChatBarComponent.altText.text = "";

        // Act
        voiceChatBarComponent.SetTalkingMessage(isSomeoneTalking, testMessage);

        // Assert
        Assert.AreEqual(testMessage, voiceChatBarComponent.model.message);
        Assert.AreEqual(isSomeoneTalking, voiceChatBarComponent.someoneTalkingContainer.gameObject.activeSelf);
        Assert.AreEqual(!isSomeoneTalking, voiceChatBarComponent.altText.gameObject.activeSelf);

        if (isSomeoneTalking)
            Assert.AreEqual(testMessage, voiceChatBarComponent.someoneTalkingText.text);
        else
            Assert.AreEqual(testMessage, voiceChatBarComponent.altText.text);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetAsJoinedCorrectly(bool isJoined)
    {
        // Arrange
        voiceChatBarComponent.model.isJoined = !isJoined;
        voiceChatBarComponent.joinedPanel.SetActive(isJoined);
        voiceChatBarComponent.startCallButton.gameObject.SetActive(!isJoined);

        // Act
        voiceChatBarComponent.SetAsJoined(isJoined);

        // Assert
        Assert.AreEqual(isJoined, voiceChatBarComponent.model.isJoined);
        Assert.AreEqual(isJoined, voiceChatBarComponent.joinedPanel.activeSelf);
        Assert.AreEqual(!isJoined, voiceChatBarComponent.startCallButton.gameObject.activeSelf);
    }
}
