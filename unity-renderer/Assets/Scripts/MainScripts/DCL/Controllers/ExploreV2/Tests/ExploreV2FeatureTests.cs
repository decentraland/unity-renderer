using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;

public class ExploreV2FeatureTests
{
    private ExploreV2Feature exploreV2Feature;
    private IExploreV2MenuComponentView exploreV2MenuView;

    [SetUp]
    public void SetUp()
    {
        exploreV2MenuView = Substitute.For<IExploreV2MenuComponentView>();
        exploreV2Feature = Substitute.ForPartsOf<ExploreV2Feature>();
        exploreV2Feature.Configure().CreateView().Returns(info => exploreV2MenuView);
        exploreV2Feature.Initialize();
    }

    [TearDown]
    public void TearDown() { exploreV2Feature.Dispose(); }

    [Test]
    public void InitializeCorrectly()
    {
        // Assert
        Assert.AreEqual(exploreV2MenuView, exploreV2Feature.view);
        exploreV2MenuView.Received().SetActive(false);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetVisibilityCorrectly(bool isVisible)
    {
        // Act
        exploreV2Feature.SetVisibility(isVisible);

        //Assert
        exploreV2MenuView.Received().SetActive(isVisible);
    }

    [Test]
    public void OnProfileUpdatedCorrectly()
    {
        // Arrange
        UserProfile testUserProfile = new UserProfile();

        // Act
        exploreV2Feature.OnProfileUpdated(testUserProfile);

        //Assert
        exploreV2MenuView.currentProfileCard.Received().SetProfileName(testUserProfile.userName);
        exploreV2MenuView.currentProfileCard.Received().SetProfileAddress(testUserProfile.ethAddress);
        exploreV2MenuView.currentProfileCard.Received().SetLoadingIndicatorVisible(true);
    }

    [Test]
    public void SetProfileImageCorrectly()
    {
        // Arrange
        Texture2D testTexture = new Texture2D(10, 10);

        // Act
        exploreV2Feature.SetProfileImage(testTexture);

        //Assert
        exploreV2MenuView.currentProfileCard.Received().SetLoadingIndicatorVisible(false);
        exploreV2MenuView.currentProfileCard.Received().SetProfilePicture(testTexture);
    }

    [Test]
    public void ClickOnCloseButtonCorrectly()
    {
        // Arrange
        bool clicked = false;
        exploreV2Feature.OnClose += () => clicked = true;

        // Act
        exploreV2Feature.OnCloseButtonPressed();

        // Assert
        Assert.IsTrue(clicked);
    }
}