using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class ProfileCardComponentViewTests
{
    private ProfileCardComponentView profileCardComponent;
    private Texture2D testTexture;

    [SetUp]
    public void SetUp()
    {
        profileCardComponent = BaseComponentView.Create<ProfileCardComponentView>("ProfileCard");
        testTexture = new Texture2D(20, 20);
    }

    [TearDown]
    public void TearDown()
    {
        profileCardComponent.Dispose();
        GameObject.Destroy(profileCardComponent.gameObject);
        GameObject.Destroy(testTexture);
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
            profilePictureSprite = Sprite.Create(testTexture, new Rect(), Vector2.zero),
            profileName = "Test name",
            profileAddress = "Test address"
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
        Sprite testPicture = Sprite.Create(testTexture, new Rect(), Vector2.zero);
        string testName = "Test name";
        string testAddress = "Test address";
        Button.ButtonClickedEvent testClickedEvent = new Button.ButtonClickedEvent();

        profileCardComponent.model.profilePictureSprite = testPicture;
        profileCardComponent.model.profileName = testName;
        profileCardComponent.model.profileAddress = testAddress;

        // Act
        profileCardComponent.RefreshControl();

        // Assert
        Assert.AreEqual(testPicture, profileCardComponent.model.profilePictureSprite, "The profile picture does not match in the model.");
        Assert.AreEqual(testName, profileCardComponent.model.profileName, "The profile name does not match in the model.");
        Assert.AreEqual(testAddress, profileCardComponent.model.profileAddress, "The profile address does not match in the model.");
    }

    [Test]
    public void SetProfilePictureFromSpriteCorrectly()
    {
        // Arrange
        Sprite testPicture = Sprite.Create(testTexture, new Rect(), Vector2.zero);

        // Act
        profileCardComponent.SetProfilePicture(testPicture);

        // Assert
        Assert.AreEqual(testPicture, profileCardComponent.model.profilePictureSprite, "The profile picture does not match in the model.");
        Assert.AreEqual(testPicture, profileCardComponent.profileImage.image.sprite, "The profile image does not match.");
    }

    [Test]
    [TestCase("TestName")]
    [TestCase(null)]
    public void SetProfileNameCorrectly(string name)
    {
        // Act
        profileCardComponent.SetProfileName(name);

        // Assert
        Assert.AreEqual(name, profileCardComponent.model.profileName, "The profile name does not match in the model.");
        if (!string.IsNullOrEmpty(name))
            Assert.AreEqual(name, profileCardComponent.profileName.text, "The profile name text does not match.");
        else
            Assert.AreEqual(string.Empty, profileCardComponent.profileName.text, "The profile name text does not match.");
    }

    [Test]
    [TestCase("1234567")]
    [TestCase("123")]
    [TestCase(null)]
    public void SetProfileAddressCorrectly(string address)
    {
        // Act
        profileCardComponent.SetProfileAddress(address);

        // Assert
        Assert.AreEqual(address, profileCardComponent.model.profileAddress, "The profile address does not match in the model.");
        if (!string.IsNullOrEmpty(address))
        {
            if (address.Length >= 4)
                Assert.AreEqual($"#{address.Substring(address.Length - 4, 4)}", profileCardComponent.profileAddress.text, "The profile address text does not match.");
            else
                Assert.AreEqual($"#{address}", profileCardComponent.profileAddress.text, "The profile address text does not match.");
        }
        else
        {
            Assert.AreEqual(string.Empty, profileCardComponent.profileAddress.text, "The profile address text does not match.");
        }
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