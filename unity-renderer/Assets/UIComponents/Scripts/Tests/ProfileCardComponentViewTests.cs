using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class ProfileCardComponentViewTests
{
    private ProfileCardComponentView profileCardComponent;

    [SetUp]
    public void SetUp() { profileCardComponent = BaseComponentView.Create<ProfileCardComponentView>("ProfileCard"); }

    [TearDown]
    public void TearDown()
    {
        profileCardComponent.Dispose();
        GameObject.Destroy(profileCardComponent.gameObject);
    }

    [Test]
    public void SetOnClickCorrectly()
    {
        // Arrange
        bool isClicked = false;
        profileCardComponent.onClick.AddListener(() => isClicked = true);

        // Act
        profileCardComponent.button.onClick.Invoke();

        // Assert
        Assert.IsTrue(isClicked, "The button has not responded to the onClick action.");
    }

    [Test]
    public void ConfigureButtonCorrectly()
    {
        // Arrange
        ProfileCardComponentModel testModel = new ProfileCardComponentModel
        {
            profilePicture = Sprite.Create(new Texture2D(10, 10), new Rect(), Vector2.zero),
            profileName = "Test name",
            profileAddress = "Test address",
            onClickEvent = new Button.ButtonClickedEvent()
        };

        // Act
        profileCardComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, profileCardComponent.model, "The model does not match after configuring the button.");
    }

    [Test]
    public void RefreshButtonCorrectly()
    {
        // Arrange
        Sprite testPicture = Sprite.Create(new Texture2D(10, 10), new Rect(), Vector2.zero);
        string testName = "Test name";
        string testAddress = "Test address";
        Button.ButtonClickedEvent testClickedEvent = new Button.ButtonClickedEvent();

        profileCardComponent.model.profilePicture = testPicture;
        profileCardComponent.model.profileName = testName;
        profileCardComponent.model.profileAddress = testAddress;
        profileCardComponent.model.onClickEvent = testClickedEvent;

        // Act
        profileCardComponent.RefreshControl();

        // Assert
        Assert.AreEqual(testPicture, profileCardComponent.model.profilePicture, "The profile picture does not match in the model.");
        Assert.AreEqual(testName, profileCardComponent.model.profileName, "The profile name does not match in the model.");
        Assert.AreEqual(testAddress, profileCardComponent.model.profileAddress, "The profile address does not match in the model.");
        Assert.AreEqual(testClickedEvent, profileCardComponent.model.onClickEvent, "The onClick event does not match in the model.");
    }

    [Test]
    public void SetProfilePictureCorrectly()
    {
        // Arrange
        Sprite testPicture = Sprite.Create(new Texture2D(10, 10), new Rect(), Vector2.zero);

        // Act
        profileCardComponent.SetProfilePicture(testPicture);

        // Assert
        Assert.AreEqual(testPicture, profileCardComponent.model.profilePicture, "The profile picture does not match in the model.");
        Assert.AreEqual(testPicture, profileCardComponent.profileImage.image.sprite, "The profile image does not match.");
    }

    [Test]
    public void SetProfileNameCorrectly()
    {
        // Arrange
        string testName = "Test";

        // Act
        profileCardComponent.SetProfileName(testName);

        // Assert
        Assert.AreEqual(testName, profileCardComponent.model.profileName, "The profile name does not match in the model.");
        Assert.AreEqual(testName, profileCardComponent.profileName.text, "The profile name text does not match.");
    }

    [Test]
    [TestCase("1234567")]
    [TestCase("123")]
    public void SetProfileAddressCorrectly(string address)
    {
        // Arrange
        string testAddress = "Test";

        // Act
        profileCardComponent.SetProfileAddress(testAddress);

        // Assert
        Assert.AreEqual(testAddress, profileCardComponent.model.profileAddress, "The profile address does not match in the model.");
        if (address.Length >= 4)
            Assert.AreEqual($"#{testAddress.Substring(testAddress.Length - 4, 4)}", profileCardComponent.profileAddress.text, "The profile address text does not match.");
        else
            Assert.AreEqual($"#{testAddress}", profileCardComponent.profileAddress.text, "The profile address text does not match.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetLoadingIndicatorVisibleCorrectly(bool isVisible)
    {
        // Arrange
        profileCardComponent.profileImage.image.enabled = isVisible;

        // Act
        profileCardComponent.SetLoadingIndicatorVisible(isVisible);

        // Assert
        Assert.AreEqual(isVisible, profileCardComponent.profileImage.loadingIndicator.activeSelf);
    }
}