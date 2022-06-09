using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

public class VoiceChatPlayerComponentViewShould
{
    private VoiceChatWindowComponentView voiceChatWindowComponentView;
    private VoiceChatPlayerComponentView voiceChatPlayerComponent;
    private UserContextMenu testUserContextMenu;

    [SetUp]
    public void SetUp()
    {
        voiceChatPlayerComponent = BaseComponentView.Create<VoiceChatPlayerComponentView>("SocialBarV1/VoiceChatPlayer");
        voiceChatPlayerComponent.avatarPreview.imageObserver = Substitute.For<ILazyTextureObserver>();
        voiceChatWindowComponentView = BaseComponentView.Create<VoiceChatWindowComponentView>("SocialBarV1/VoiceChatHUD");
        testUserContextMenu = voiceChatWindowComponentView.contextMenuPanel;
    }

    [TearDown]
    public void TearDown()
    {
        voiceChatPlayerComponent.Dispose();
        voiceChatWindowComponentView.Dispose();
    }

    [Test]
    public void ConfigureCorrectly()
    {
        // Arrange
        VoiceChatPlayerComponentModel testModel = new VoiceChatPlayerComponentModel
        {
            isBackgroundHover = false,
            isBlocked = false,
            isFriend = true,
            isJoined = true,
            isMuted = false,
            isTalking = false,
            userId = "TestId",
            userImageUrl = "",
            userName = "TestName"
        };

        // Act
        voiceChatPlayerComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, voiceChatPlayerComponent.model);
    }

    [Test]
    public void SetUserIdCorrectly()
    {
        // Arrange
        string testUserId = "TestId";
        voiceChatPlayerComponent.model.userId = "";

        // Act
        voiceChatPlayerComponent.SetUserId(testUserId);

        // Assert
        Assert.AreEqual(testUserId, voiceChatPlayerComponent.model.userId);
    }

    [Test]
    public void SetUserImageCorrectly()
    {
        // Arrange
        string testUri = "testUri";
        voiceChatPlayerComponent.model.userImageUrl = null;

        // Act
        voiceChatPlayerComponent.SetUserImage(testUri);

        // Assert
        Assert.AreEqual(testUri, voiceChatPlayerComponent.model.userImageUrl);
        voiceChatPlayerComponent.avatarPreview.imageObserver.Received().RefreshWithUri(testUri);
    }

    [Test]
    public void SetUserNameCorrectly()
    {
        // Arrange
        string testUserName = "Test name";

        // Act
        voiceChatPlayerComponent.SetUserName(testUserName);

        // Assert
        Assert.AreEqual(testUserName, voiceChatPlayerComponent.model.userName);
        Assert.AreEqual(testUserName, voiceChatPlayerComponent.userName.text);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetAsMutedCorrectly(bool isMuted)
    {
        // Arrange
        voiceChatPlayerComponent.muteButton.gameObject.SetActive(isMuted);
        voiceChatPlayerComponent.unmuteButton.gameObject.SetActive(!isMuted);

        // Act
        voiceChatPlayerComponent.SetAsMuted(isMuted);

        // Assert
        Assert.AreEqual(isMuted, voiceChatPlayerComponent.model.isMuted);
        Assert.AreEqual(!isMuted, voiceChatPlayerComponent.muteButton.gameObject.activeSelf);
        Assert.AreEqual(isMuted, voiceChatPlayerComponent.unmuteButton.gameObject.activeSelf);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetAsTalkingCorrectly(bool isTalking)
    {
        // Arrange
        voiceChatPlayerComponent.muteButtonImage.color = Color.green;

        // Act
        voiceChatPlayerComponent.SetAsTalking(isTalking);

        // Assert
        Assert.AreEqual(isTalking, voiceChatPlayerComponent.model.isTalking);
        Assert.AreEqual(isTalking ? voiceChatPlayerComponent.talkingColor : voiceChatPlayerComponent.nonTalkingColor, voiceChatPlayerComponent.muteButtonImage.color);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetAsBlockedCorrectly(bool isBlocked)
    {
        // Arrange
        voiceChatPlayerComponent.blockedGO.gameObject.SetActive(!isBlocked);

        // Act
        voiceChatPlayerComponent.SetAsBlocked(isBlocked);

        // Assert
        Assert.AreEqual(isBlocked, voiceChatPlayerComponent.model.isBlocked);
        Assert.AreEqual(isBlocked, voiceChatPlayerComponent.blockedGO.gameObject.activeSelf);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetAsFriendCorrectly(bool isFriend)
    {
        // Arrange
        voiceChatPlayerComponent.friendLabel.gameObject.SetActive(!isFriend);

        // Act
        voiceChatPlayerComponent.SetAsFriend(isFriend);

        // Assert
        Assert.AreEqual(isFriend, voiceChatPlayerComponent.model.isFriend);
        Assert.AreEqual(isFriend, voiceChatPlayerComponent.friendLabel.gameObject.activeSelf);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetAsJoinedCorrectly(bool isJoined)
    {
        // Arrange
        voiceChatPlayerComponent.buttonsContainer.SetActive(!isJoined);

        // Act
        voiceChatPlayerComponent.SetAsJoined(isJoined);

        // Assert
        Assert.AreEqual(isJoined, voiceChatPlayerComponent.model.isJoined);
        Assert.AreEqual(isJoined, voiceChatPlayerComponent.buttonsContainer.activeSelf);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetBackgroundHoverCorrectly(bool isHover)
    {
        // Arrange
        voiceChatPlayerComponent.backgroundHover.SetActive(!isHover);
        voiceChatPlayerComponent.menuButton.gameObject.SetActive(!isHover);

        // Act
        voiceChatPlayerComponent.SetBackgroundHover(isHover);

        // Assert
        Assert.AreEqual(isHover, voiceChatPlayerComponent.model.isBackgroundHover);
        Assert.AreEqual(isHover, voiceChatPlayerComponent.backgroundHover.activeSelf);
        Assert.AreEqual(isHover, voiceChatPlayerComponent.menuButton.gameObject.activeSelf);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetActiveCorrectly(bool isActive)
    {
        // Arrange
        voiceChatPlayerComponent.gameObject.SetActive(!isActive);

        // Act
        voiceChatPlayerComponent.SetActive(isActive);

        // Assert
        Assert.AreEqual(isActive, voiceChatPlayerComponent.gameObject.activeSelf);
    }

    [Test]
    public void DockAndOpenUserContextMenuCorrectly()
    {
        // Arrange
        var testPanelTransform = (RectTransform)testUserContextMenu.transform;
        testPanelTransform.pivot = Vector2.zero;
        testPanelTransform.position = Vector3.zero;

        // Act
        voiceChatPlayerComponent.DockAndOpenUserContextMenu(testUserContextMenu);

        // Assert
        Assert.AreEqual(voiceChatPlayerComponent.menuPositionReference.pivot, testPanelTransform.pivot);
        Assert.AreEqual(voiceChatPlayerComponent.menuPositionReference.position, testPanelTransform.position);
    }

    [Test]
    public void OnFocusCorrectly()
    {
        // Arrange
        voiceChatPlayerComponent.backgroundHover.SetActive(false);
        voiceChatPlayerComponent.menuButton.gameObject.SetActive(false);

        // Act
        voiceChatPlayerComponent.OnFocus();

        // Assert
        Assert.IsTrue(voiceChatPlayerComponent.model.isBackgroundHover);
        Assert.IsTrue(voiceChatPlayerComponent.backgroundHover.activeSelf);
        Assert.IsTrue(voiceChatPlayerComponent.menuButton.gameObject.activeSelf);
    }

    [Test]
    public void OnLoseFocusCorrectly()
    {
        // Arrange
        voiceChatPlayerComponent.backgroundHover.SetActive(true);
        voiceChatPlayerComponent.menuButton.gameObject.SetActive(true);

        // Act
        voiceChatPlayerComponent.OnLoseFocus();

        // Assert
        Assert.IsFalse(voiceChatPlayerComponent.model.isBackgroundHover);
        Assert.IsFalse(voiceChatPlayerComponent.backgroundHover.activeSelf);
        Assert.IsFalse(voiceChatPlayerComponent.menuButton.gameObject.activeSelf);
    }
}
