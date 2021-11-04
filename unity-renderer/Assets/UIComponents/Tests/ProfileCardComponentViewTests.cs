using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

public class ProfileCardComponentViewTests
{
    private ProfileCardComponentView profileCardComponent;
    private Texture2D testTexture;
    private Sprite testSprite;

    [SetUp]
    public void SetUp()
    {
        profileCardComponent = BaseComponentView.Create<ProfileCardComponentView>("ProfileCard");
        profileCardComponent.profileImage.imageObserver = Substitute.For<ILazyTextureObserver>();
        testTexture = new Texture2D(20, 20);
        testSprite = Sprite.Create(testTexture, new Rect(), Vector2.zero);
    }

    [TearDown]
    public void TearDown()
    {
        profileCardComponent.Dispose();
        GameObject.Destroy(profileCardComponent.gameObject);
        GameObject.Destroy(testTexture);
        GameObject.Destroy(testSprite);
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
            profilePictureSprite = testSprite,
            profileName = "Test name",
            profileAddress = "Test address"
        };

        // Act
        profileCardComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, profileCardComponent.model, "The model does not match after configuring the button.");
    }

    [Test]
    public void SetProfilePictureFromSpriteCorrectly()
    {
        // Act
        profileCardComponent.SetProfilePicture(testSprite);

        // Assert
        Assert.AreEqual(testSprite, profileCardComponent.model.profilePictureSprite, "The profile picture sprite does not match in the model.");
        Assert.AreEqual(testSprite, profileCardComponent.profileImage.image.sprite, "The profile image does not match.");
    }

    [Test]
    public void SetProfilePictureFromTextureCorrectly()
    {
        // Arrange
        profileCardComponent.model.profilePictureTexture = null;

        // Act
        profileCardComponent.SetProfilePicture(testTexture);

        // Assert
        Assert.AreEqual(testTexture, profileCardComponent.model.profilePictureTexture, "The profile picture texture does not match in the model.");
        profileCardComponent.profileImage.imageObserver.Received().RefreshWithTexture(testTexture);
    }

    [Test]
    public void SetProfilePictureFromUriCorrectly()
    {
        // Arrange
        string testUri = "testUri";
        profileCardComponent.model.profilePictureUri = null;

        // Act
        profileCardComponent.SetProfilePicture(testUri);

        // Assert
        Assert.AreEqual(testUri, profileCardComponent.model.profilePictureUri, "The profile picture uri does not match in the model.");
        profileCardComponent.profileImage.imageObserver.Received().RefreshWithUri(testUri);
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

    [Test]
    public void CallOnProfileImageLoadedCorrectly()
    {
        // Act
        profileCardComponent.OnProfileImageLoaded(testSprite);

        // Assert
        Assert.AreEqual(testSprite, profileCardComponent.model.profilePictureSprite, "The profile picture sprite does not match in the model.");
        Assert.AreEqual(testSprite, profileCardComponent.profileImage.image.sprite, "The profile image does not match.");
    }
}