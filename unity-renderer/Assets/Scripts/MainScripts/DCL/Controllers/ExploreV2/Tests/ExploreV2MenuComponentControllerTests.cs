using DCL;
using ExploreV2Analytics;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Variables.RealmsInfo;

public class ExploreV2MenuComponentControllerTests
{
    private ExploreV2MenuComponentController exploreV2MenuController;
    private IExploreV2MenuComponentView exploreV2MenuView;
    private IExploreV2Analytics exploreV2Analytics;

    [SetUp]
    public void SetUp()
    {
        exploreV2MenuView = Substitute.For<IExploreV2MenuComponentView>();
        exploreV2Analytics = Substitute.For<IExploreV2Analytics>();
        exploreV2MenuController = Substitute.ForPartsOf<ExploreV2MenuComponentController>();
        exploreV2MenuController.Configure().CreateView().Returns(info => exploreV2MenuView);
        exploreV2MenuController.Configure().CreateAnalyticsController().Returns(info => exploreV2Analytics);
        exploreV2MenuController.Initialize();
    }

    [TearDown]
    public void TearDown()
    {
        DataStore.i.common.isTutorialRunning.Set(false);
        exploreV2MenuController.Dispose();
    }

    [Test]
    public void InitializeCorrectly()
    {
        // Assert
        Assert.AreEqual(exploreV2Analytics, exploreV2MenuController.exploreV2Analytics);
        Assert.AreEqual(exploreV2MenuView, exploreV2MenuController.view);
        exploreV2MenuView.Received().SetVisible(false);
        Assert.IsTrue(DataStore.i.exploreV2.isInitialized.Get());
    }

    [Test]
    public void CreateControllersCorrectly()
    {
        // Arrange
        exploreV2MenuController.placesAndEventsSectionController = null;

        // Act
        exploreV2MenuController.InitializePlacesAndEventsSection();

        // Assert
        Assert.IsNotNull(exploreV2MenuController.placesAndEventsSectionController);
    }

    [Test]
    [TestCase(ExploreSection.Explore)]
    [TestCase(ExploreSection.Quest)]
    [TestCase(ExploreSection.Backpack)]
    [TestCase(ExploreSection.Builder)]
    [TestCase(ExploreSection.Map)]
    [TestCase(ExploreSection.Settings)]
    public void RaiseOnSectionOpenCorrectly(ExploreSection section)
    {
        // Arrange
        exploreV2MenuController.currentOpenSection = ExploreSection.Settings;

        // Act
        exploreV2MenuController.OnSectionOpen(section);

        // Assert
        exploreV2Analytics.Received().SendStartMenuSectionVisibility(Arg.Any<ExploreSection>(), false);
        Assert.AreEqual(section, exploreV2MenuController.currentOpenSection);

        if (section == ExploreSection.Backpack)
        {
            exploreV2MenuView.Received()
                             .ConfigureEncapsulatedSection(
                                 ExploreSection.Backpack,
                                 DataStore.i.exploreV2.configureBackpackInFullscreenMenu);
        }

        Assert.AreEqual(section == ExploreSection.Explore, exploreV2MenuController.placesAndEventsVisible.Get());
        Assert.AreEqual(section == ExploreSection.Backpack, exploreV2MenuController.avatarEditorVisible.Get());
        Assert.AreEqual(section == ExploreSection.Map, exploreV2MenuController.navmapVisible.Get());
        Assert.AreEqual(section == ExploreSection.Builder, exploreV2MenuController.builderVisible.Get());
        Assert.AreEqual(section == ExploreSection.Quest, exploreV2MenuController.questVisible.Get());
        Assert.AreEqual(section == ExploreSection.Settings, exploreV2MenuController.settingsVisible.Get());
        Assert.IsFalse(exploreV2MenuController.profileCardIsOpen.Get());
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
    }

    [TestCase(true)]
    [TestCase(false)]
    public void RaiseIsOpenChangedCorrectly(bool isVisible)
    {
        // Arrange
        DataStore.i.common.isTutorialRunning.Set(true);

        // Act
        exploreV2MenuController.IsOpenChanged(isVisible, !isVisible);

        // Assert
        if (isVisible)
            exploreV2MenuView.Received().GoToSection(ExploreV2MenuComponentView.DEFAULT_SECTION);
        else
        {
            Assert.IsFalse(exploreV2MenuController.placesAndEventsVisible.Get());
            Assert.IsFalse(exploreV2MenuController.avatarEditorVisible.Get());
            Assert.IsFalse(exploreV2MenuController.profileCardIsOpen.Get());
            Assert.IsFalse(exploreV2MenuController.navmapVisible.Get());
            Assert.IsFalse(exploreV2MenuController.builderVisible.Get());
            Assert.IsFalse(exploreV2MenuController.questVisible.Get());
            Assert.IsFalse(exploreV2MenuController.settingsVisible.Get());
        }

        exploreV2MenuView.Received().SetVisible(isVisible);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void RaisePlacesAndEventsVisibleChangedCorrectly(bool isVisible)
    {
        // Arrange
        exploreV2MenuController.isInitialized.Set(true);
        exploreV2MenuController.currentOpenSection = ExploreSection.Explore;

        // Act
        exploreV2MenuController.PlacesAndEventsVisibleChanged(isVisible, !isVisible);

        // Assert
        if (isVisible)
            exploreV2MenuView.Received().GoToSection(ExploreSection.Explore);

        Assert.AreEqual(isVisible, DataStore.i.exploreV2.isOpen.Get());
        exploreV2Analytics.Received().SendStartMenuSectionVisibility(ExploreSection.Explore, isVisible);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void RaiseIsAvatarEditorInitializedChangedCorrectly(bool isVisible)
    {
        // Act
        exploreV2MenuController.IsAvatarEditorInitializedChanged(isVisible, !isVisible);

        // Assert
        exploreV2MenuView.Received().SetSectionActive(ExploreSection.Backpack, isVisible);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void RaiseAvatarEditorVisibleChangedCorrectly(bool isVisible)
    {
        // Arrange
        exploreV2MenuController.isAvatarEditorInitialized.Set(true);
        DataStore.i.common.isSignUpFlow.Set(false);
        exploreV2MenuController.currentOpenSection = ExploreSection.Backpack;

        // Act
        exploreV2MenuController.AvatarEditorVisibleChanged(isVisible, !isVisible);

        // Assert
        if (isVisible)
            exploreV2MenuView.Received().GoToSection(ExploreSection.Backpack);

        Assert.AreEqual(isVisible, DataStore.i.exploreV2.isOpen.Get());
        exploreV2Analytics.Received().SendStartMenuSectionVisibility(ExploreSection.Backpack, isVisible);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void RaiseIsNavMapInitializedChangedCorrectly(bool isVisible)
    {
        // Act
        exploreV2MenuController.IsNavMapInitializedChanged(isVisible, !isVisible);

        // Assert
        exploreV2MenuView.Received().SetSectionActive(ExploreSection.Map, isVisible);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void RaiseNavmapVisibleChangedCorrectly(bool isVisible)
    {
        // Arrange
        exploreV2MenuController.isNavmapInitialized.Set(true);
        exploreV2MenuController.currentOpenSection = ExploreSection.Map;

        // Act
        exploreV2MenuController.NavmapVisibleChanged(isVisible, !isVisible);

        // Assert
        if (isVisible)
            exploreV2MenuView.Received().GoToSection(ExploreSection.Map);

        Assert.AreEqual(isVisible, DataStore.i.exploreV2.isOpen.Get());
        exploreV2Analytics.Received().SendStartMenuSectionVisibility(ExploreSection.Map, isVisible);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void RaiseIsBuilderInitializedChangedCorrectly(bool isVisible)
    {
        // Act
        exploreV2MenuController.IsBuilderInitializedChanged(isVisible, !isVisible);

        // Assert
        exploreV2MenuView.Received().SetSectionActive(ExploreSection.Builder, isVisible);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void RaiseBuilderVisibleChangedCorrectly(bool isVisible)
    {
        // Arrange
        exploreV2MenuController.isBuilderInitialized.Set(true);
        exploreV2MenuController.currentOpenSection = ExploreSection.Builder;

        // Act
        exploreV2MenuController.BuilderVisibleChanged(isVisible, !isVisible);

        // Assert
        if (isVisible)
            exploreV2MenuView.Received().GoToSection(ExploreSection.Builder);

        Assert.AreEqual(isVisible, DataStore.i.exploreV2.isOpen.Get());
        exploreV2Analytics.Received().SendStartMenuSectionVisibility(ExploreSection.Builder, isVisible);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void RaiseIsQuestInitializedChangedCorrectly(bool isVisible)
    {
        // Act
        exploreV2MenuController.IsQuestInitializedChanged(isVisible, !isVisible);

        // Assert
        exploreV2MenuView.Received().SetSectionActive(ExploreSection.Quest, isVisible);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void RaiseQuestVisibleChangedCorrectly(bool isVisible)
    {
        // Arrange
        exploreV2MenuController.isQuestInitialized.Set(true);
        exploreV2MenuController.currentOpenSection = ExploreSection.Quest;

        // Act
        exploreV2MenuController.QuestVisibleChanged(isVisible, !isVisible);

        // Assert
        if (isVisible)
            exploreV2MenuView.Received().GoToSection(ExploreSection.Quest);

        Assert.AreEqual(isVisible, DataStore.i.exploreV2.isOpen.Get());
        exploreV2Analytics.Received().SendStartMenuSectionVisibility(ExploreSection.Quest, isVisible);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void RaiseIsSettingsPanelInitializedChangedCorrectly(bool isVisible)
    {
        // Act
        exploreV2MenuController.IsSettingsPanelInitializedChanged(isVisible, !isVisible);

        // Assert
        exploreV2MenuView.Received().SetSectionActive(ExploreSection.Settings, isVisible);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void RaiseSettingsVisibleChangedCorrectly(bool isVisible)
    {
        // Arrange
        exploreV2MenuController.isSettingsPanelInitialized.Set(true);
        exploreV2MenuController.currentOpenSection = ExploreSection.Settings;

        // Act
        exploreV2MenuController.SettingsVisibleChanged(isVisible, !isVisible);

        // Assert
        if (isVisible)
            exploreV2MenuView.Received().GoToSection(ExploreSection.Settings);

        Assert.AreEqual(isVisible, DataStore.i.exploreV2.isOpen.Get());
        exploreV2Analytics.Received().SendStartMenuSectionVisibility(ExploreSection.Settings, isVisible);
    }

    [Test]
    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    public void RaiseCurrentSectionIndexChangedCorrectly(int sectionIndex)
    {
        // Arrange
        DataStore.i.exploreV2.isInShowAnimationTransiton.Set(false);

        // Act
        exploreV2MenuController.CurrentSectionIndexChanged(sectionIndex, 0);

        // Assert
        exploreV2MenuView.Received().GoToSection(Arg.Any<ExploreSection>());
    }

    [Test]
    public void UpdateRealmInfoCorrectly()
    {
        // Arrange
        string testRealmName = "TestName";
        DataStore.i.realm.playerRealm.Set(new CurrentRealmModel
        {
            serverName = testRealmName,
            layer = null
        });

        List<RealmModel> testRealmList = new List<RealmModel>();
        int testUsersCount = 100;
        testRealmList.Add(new RealmModel
        {
            serverName = testRealmName,
            layer = null,
            usersCount = testUsersCount
        });
        DataStore.i.realm.realmsInfo.Set(testRealmList.ToArray());

        // Act
        exploreV2MenuController.UpdateRealmInfo(DataStore.i.realm.playerRealm.Get(), null);

        // Assert
        exploreV2MenuView.currentRealmViewer.Received().SetRealm(testRealmName);
        exploreV2MenuView.currentRealmSelectorModal.Received().SetCurrentRealm(testRealmName);
        exploreV2MenuView.currentRealmViewer.Received().SetNumberOfUsers(testUsersCount);
    }

    [Test]
    public void UpdateAvailableRealmsInfoCorrectly()
    {
        // Arrange
        RealmModel[] testRealms = 
        { 
            new RealmModel
            {
                serverName = "TestRealm1",
                usersCount = 10
            },
            new RealmModel
            {
                serverName = "TestRealm2",
                usersCount = 20
            },
            new RealmModel
            {
                serverName = "TestRealm3",
                usersCount = 30
            }
        };

        // Act
        exploreV2MenuController.UpdateAvailableRealmsInfo(testRealms);

        // Assert
        Assert.AreEqual(3, exploreV2MenuController.currentAvailableRealms.Count);
        exploreV2MenuView.currentRealmSelectorModal.Received().SetAvailableRealms(exploreV2MenuController.currentAvailableRealms);
    }

    [Test]
    public void UpdateProfileInfoCorrectly()
    {
        // Arrange
        UserProfile testUserProfile = new UserProfile();

        // Act
        exploreV2MenuController.UpdateProfileInfo(testUserProfile);

        //Assert
        exploreV2MenuView.currentProfileCard.Received().SetIsClaimedName(testUserProfile.hasClaimedName);
        exploreV2MenuView.currentProfileCard.Received().SetProfileName(testUserProfile.userName);
        exploreV2MenuView.currentProfileCard.Received().SetProfileAddress(testUserProfile.ethAddress);
        exploreV2MenuView.currentProfileCard.Received().SetProfilePicture(testUserProfile.face256SnapshotURL);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ClickOnCloseButtonCorrectly(bool fromShortcut)
    {
        // Arrange
        exploreV2MenuController.isOpen.Set(true);
        DataStore.i.exploreV2.isSomeModalOpen.Set(false);

        // Act
        exploreV2MenuController.OnCloseButtonPressed(fromShortcut);

        // Assert
        exploreV2Analytics.Received().SendStartMenuVisibility(false, fromShortcut ? ExploreUIVisibilityMethod.FromShortcut : ExploreUIVisibilityMethod.FromClick);
        Assert.IsFalse(DataStore.i.exploreV2.isOpen.Get());
    }
}