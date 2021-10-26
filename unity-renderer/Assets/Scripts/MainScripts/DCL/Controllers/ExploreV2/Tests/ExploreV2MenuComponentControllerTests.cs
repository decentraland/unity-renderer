using DCL;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System.Collections.Generic;
using Variables.RealmsInfo;

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
        exploreV2MenuView.Received().SetVisible(false);
        Assert.IsTrue(DataStore.i.exploreV2.isInitialized.Get());
        Assert.IsNotNull(exploreV2MenuController.toggleExploreTrigger);
    }

    [Test]
    public void CreateControllersCorrectly()
    {
        // Arrange
        exploreV2MenuController.placesAndEventsSectionController = null;

        // Act
        exploreV2MenuController.CreateControllers();

        // Assert
        Assert.IsNotNull(exploreV2MenuController.placesAndEventsSectionController);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetVisibilityCorrectly(bool isVisible)
    {
        // Arrange
        DataStore.i.exploreV2.isOpen.Set(!isVisible);

        // Act
        exploreV2MenuController.SetVisibility(isVisible);

        //Assert
        Assert.AreEqual(isVisible, DataStore.i.exploreV2.isOpen.Get());
        exploreV2MenuView.Received().SetVisible(isVisible);
    }

    [Test]
    public void UpdateRealmInfoCorrectly()
    {
        // Arrange
        string testRealmName = "TestName";
        string testRealmLayer = "TestLayer";
        DataStore.i.playerRealm.Set(new CurrentRealmModel
        {
            serverName = testRealmName,
            layer = testRealmLayer
        });

        List<RealmModel> testRealmList = new List<RealmModel>();
        int testUsersCount = 100;
        testRealmList.Add(new RealmModel
        {
            serverName = testRealmName,
            layer = testRealmLayer,
            usersCount = testUsersCount
        });
        DataStore.i.realmsInfo.Set(testRealmList.ToArray());

        // Act
        exploreV2MenuController.UpdateRealmInfo(DataStore.i.playerRealm.Get(), null);

        //Assert
        exploreV2MenuView.currentRealmViewer.Received().SetRealm($"{testRealmName}-{testRealmLayer}");
        exploreV2MenuView.currentRealmViewer.Received().SetNumberOfUsers(testUsersCount);
    }

    [Test]
    public void UpdateProfileInfoCorrectly()
    {
        // Arrange
        UserProfile testUserProfile = new UserProfile();

        // Act
        exploreV2MenuController.UpdateProfileInfo(testUserProfile);

        //Assert
        exploreV2MenuView.currentProfileCard.Received().SetProfileName(testUserProfile.userName);
        exploreV2MenuView.currentProfileCard.Received().SetProfileAddress(testUserProfile.ethAddress);
        exploreV2MenuView.currentProfileCard.Received().SetProfilePicture(testUserProfile.face128SnapshotURL);
    }

    [Test]
    public void ClickOnCloseButtonCorrectly()
    {
        // Act
        exploreV2MenuController.OnCloseButtonPressed();

        // Assert
        Assert.IsFalse(DataStore.i.exploreV2.isOpen.Get());
        exploreV2MenuView.Received().SetVisible(false);
    }

    [Test]
    public void RaiseOnToggleActionTriggeredCorrectly()
    {
        // Act
        exploreV2MenuController.OnToggleActionTriggered(new DCLAction_Trigger());

        // Assert
        exploreV2MenuView.Received().SetVisible(!DataStore.i.exploreV2.isOpen.Get());
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ActivateFromTaskbarCorrectly(bool isActive)
    {
        // Act
        exploreV2MenuController.OnActivateFromTaskbar(isActive, !isActive);

        // Assert
        Assert.AreEqual(isActive, DataStore.i.exploreV2.isOpen.Get());
        exploreV2MenuView.Received().SetVisible(isActive);
    }
}