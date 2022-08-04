using NUnit.Framework;

public class JoinChannelComponentViewShould
{
    private JoinChannelComponentView joinChannelComponentView;

    [SetUp]
    public void Setup()
    {
        joinChannelComponentView = JoinChannelComponentView.Create();
    }

    [TearDown]
    public void TearDown()
    {
        joinChannelComponentView.Dispose();
    }

    [Test]
    public void ConfigureRealmSelectorCorrectly()
    {
        // Arrange
        JoinChannelComponentModel testModel = new JoinChannelComponentModel
        {
            channelId = "TestId"
        };

        // Act
        joinChannelComponentView.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, joinChannelComponentView.model, "The model does not match after configuring the realm selector.");
    }

    [Test]
    public void SetChannelCorrectly()
    {
        // Arrange
        string testName = "TestName";

        // Act
        joinChannelComponentView.SetChannel(testName);

        // Assert
        Assert.AreEqual(testName, joinChannelComponentView.model.channelId, "The channel id does not match in the model.");
        Assert.AreEqual(joinChannelComponentView.titleText.text, string.Format(JoinChannelComponentView.MODAL_TITLE, testName));
    }
}
