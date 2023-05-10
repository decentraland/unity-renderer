using DCL.Chat.HUD;
using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

public class ChannelMemberEntryShould
{
    private ChannelMemberEntry channelMemberEntryComponent;

    [SetUp]
    public void SetUp()
    {
        channelMemberEntryComponent = GameObject.Instantiate(Resources.Load<ChannelMemberEntry>("SocialBarV1/ChannelMemberEntry"));
        channelMemberEntryComponent.userThumbnail.imageObserver = Substitute.For<ILazyTextureObserver>();
    }

    [TearDown]
    public void TearDown()
    {
        channelMemberEntryComponent.Dispose();
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ConfigureEntryCorrectly(bool isOnline)
    {
        // Arrange
        ChannelMemberEntryModel testModel = new ChannelMemberEntryModel
        {
            userId = "testId",
            userName = "testName",
            thumnailUrl = "testUri",
            isOnline = isOnline
        };

        // Act
        channelMemberEntryComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, channelMemberEntryComponent.model);
        Assert.AreEqual(testModel.userName, channelMemberEntryComponent.nameLabel.text);
        Assert.AreEqual(isOnline, channelMemberEntryComponent.onlineMark.activeSelf);
        Assert.AreEqual(!isOnline, channelMemberEntryComponent.offlineMark.activeSelf);
    }
}
