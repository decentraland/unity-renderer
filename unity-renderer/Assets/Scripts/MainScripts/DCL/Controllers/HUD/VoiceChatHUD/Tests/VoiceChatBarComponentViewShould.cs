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
    public void ShowCorrectly()
    {
        // Arrange
        voiceChatBarComponent.gameObject.SetActive(false);

        // Act
        voiceChatBarComponent.Show();

        // Assert
        Assert.IsTrue(voiceChatBarComponent.gameObject.activeSelf);
    }

    [Test]
    public void HideCorrectly()
    {
        // Arrange
        voiceChatBarComponent.gameObject.SetActive(true);

        // Act
        voiceChatBarComponent.Hide();

        // Assert
        Assert.IsFalse(voiceChatBarComponent.gameObject.activeSelf);
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
}
