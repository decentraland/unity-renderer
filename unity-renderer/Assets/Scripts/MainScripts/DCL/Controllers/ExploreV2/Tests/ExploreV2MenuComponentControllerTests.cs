using DCL;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;

public class ExploreV2MenuComponentControllerTests
{
    private ExploreV2MenuComponentController exploreV2MenuController;
    private IExploreV2MenuComponentView exploreV2MenuView;

    [SetUp]
    public void SetUp()
    {
        exploreV2MenuView = Substitute.For<IExploreV2MenuComponentView>();
        exploreV2MenuController = Substitute.ForPartsOf<ExploreV2MenuComponentController>();
        exploreV2MenuController.Configure().CreateView().Returns(info => exploreV2MenuView);
        exploreV2MenuController.Initialize();
    }

    [TearDown]
    public void TearDown() { exploreV2MenuController.Dispose(); }

    [Test]
    public void InitializeCorrectly()
    {
        // Assert
        Assert.AreEqual(exploreV2MenuView, exploreV2MenuController.view);
        exploreV2MenuView.Received().SetActive(false);
        Assert.AreEqual(exploreV2MenuController, DataStore.i.exploreV2.controller.Get());
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetVisibilityCorrectly(bool isVisible)
    {
        // Act
        exploreV2MenuController.SetVisibility(isVisible);

        //Assert
        exploreV2MenuView.Received().SetActive(isVisible);
    }

    [Test]
    public void OnProfileUpdatedCorrectly()
    {
        // Arrange
        UserProfile testUserProfile = new UserProfile();

        // Act
        exploreV2MenuController.OnProfileUpdated(testUserProfile);

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
        exploreV2MenuController.SetProfileImage(testTexture);

        //Assert
        exploreV2MenuView.currentProfileCard.Received().SetLoadingIndicatorVisible(false);
        exploreV2MenuView.currentProfileCard.Received().SetProfilePicture(testTexture);
    }

    [Test]
    public void ClickOnCloseButtonCorrectly()
    {
        // Arrange
        bool clicked = false;
        exploreV2MenuController.OnClose += () => clicked = true;

        // Act
        exploreV2MenuController.OnCloseButtonPressed();

        // Assert
        Assert.IsTrue(clicked);
    }
}