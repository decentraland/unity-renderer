using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

public class FriendHeadForPlaceCardComponentViewTests
{
    private FriendHeadForPlaceCardComponentView friendHeadForPlaceCardComponent;
    private Texture2D testTexture;

    [SetUp]
    public void SetUp()
    {
        friendHeadForPlaceCardComponent = BaseComponentView.Create<FriendHeadForPlaceCardComponentView>("Sections/PlacesAndEventsSection/PlacesSubSection/FriendHeadForPlaceCard");
        friendHeadForPlaceCardComponent.friendPortrait.imageObserver = Substitute.For<ILazyTextureObserver>();
        testTexture = new Texture2D(20, 20);
    }

    [TearDown]
    public void TearDown()
    {
        friendHeadForPlaceCardComponent.Dispose();
        GameObject.Destroy(friendHeadForPlaceCardComponent.gameObject);
        GameObject.Destroy(testTexture);
    }

    [Test]
    public void ConfigureFriendHeadForPlaceCardCorrectly()
    {
        // Arrange
        FriendHeadForPlaceCardComponentModel testModel = new FriendHeadForPlaceCardComponentModel
        {
            backgroundColor = Color.green,
            userProfile = new UserProfile()
        };

        // Act
        friendHeadForPlaceCardComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, friendHeadForPlaceCardComponent.model, "The model does not match after configuring the friend head for place card.");
    }

    [Test]
    public void RaiseOnFocusCorrectly()
    {
        // Arrange
        friendHeadForPlaceCardComponent.model = new FriendHeadForPlaceCardComponentModel { userProfile = new UserProfile() };
        friendHeadForPlaceCardComponent.friendNameShowHideAnimator.gameObject.SetActive(false);

        // Act
        friendHeadForPlaceCardComponent.OnFocus();

        // Assert
        Assert.IsTrue(friendHeadForPlaceCardComponent.friendNameShowHideAnimator.gameObject.activeSelf);

        friendHeadForPlaceCardComponent.OnLoseFocus();
    }

    [Test]
    public void SetBackgroundColorCorrectly()
    {
        // Arrange
        Color testColor = Color.green;

        // Act
        friendHeadForPlaceCardComponent.SetBackgroundColor(testColor);

        // Assert
        Assert.AreEqual(testColor, friendHeadForPlaceCardComponent.model.backgroundColor, "The backgroundColor does not match in the model.");
        Assert.AreEqual(testColor, friendHeadForPlaceCardComponent.friendBackground.color, "The backgroundColor does not match.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetUserProfileCorrectly(bool isUserProfileNull)
    {
        // Arrange
        string testName = isUserProfileNull ? "" : "Test Name";
        UserProfile testUserProfile = isUserProfileNull ? null : new UserProfile();

        if (!isUserProfileNull)
            testUserProfile.UpdateData(new UserProfileModel { name = testName });

        // Act
        friendHeadForPlaceCardComponent.SetUserProfile(testUserProfile);

        // Assert
        Assert.AreEqual(testUserProfile, friendHeadForPlaceCardComponent.model.userProfile, "The user profile does not match in the model.");
        Assert.AreEqual(testName, friendHeadForPlaceCardComponent.friendName.text, "The backgroundColor does not match.");
    }

    [Test]
    public void RasieOnFaceSnapshotLoadedCorrectly()
    {
        // Act
        friendHeadForPlaceCardComponent.OnFaceSnapshotLoaded(testTexture);

        // Assert
        friendHeadForPlaceCardComponent.friendPortrait.imageObserver.Received().RefreshWithTexture(testTexture);
    }
}