using System;
using System.Collections.Generic;
using DCL;
using ExploreV2Analytics;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using Variables.RealmsInfo;

public class ExploreV2MenuComponentControllerTests
{
    private IExploreV2Analytics exploreV2Analytics;
    private ExploreV2MenuComponentController exploreV2MenuController;
    private IExploreV2MenuComponentView exploreV2MenuView;

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
        exploreV2MenuController.SetVisibilityOnOpenChanged(isVisible, !isVisible);

        // Assert
        if (isVisible)
            exploreV2MenuView.Received().GoToSection(ExploreV2MenuComponentController.DEFAULT_SECTION);
        else
        {
            Assert.IsFalse(exploreV2MenuController.placesAndEventsVisible.Get());
            Assert.IsFalse(exploreV2MenuController.avatarEditorVisible.Get());
            Assert.IsFalse(exploreV2MenuController.profileCardIsOpen.Get());
            Assert.IsFalse(exploreV2MenuController.navmapVisible.Get());
            Assert.IsFalse(exploreV2MenuController.questVisible.Get());
            Assert.IsFalse(exploreV2MenuController.settingsVisible.Get());
        }

        exploreV2MenuView.Received().SetVisible(isVisible);
    }

    [TestCase(ExploreSection.Explore,true)]
    [TestCase(ExploreSection.Explore, false)]
    [TestCase(ExploreSection.Map, true)]
    [TestCase(ExploreSection.Map,false)]
    [TestCase(ExploreSection.Backpack, true)]
    [TestCase(ExploreSection.Backpack,false)]
    [TestCase(ExploreSection.Quest, true)]
    [TestCase(ExploreSection.Quest,false)]
    [TestCase(ExploreSection.Settings, true)]
    [TestCase(ExploreSection.Settings,false)]
    public void RaiseSectionVisibleChangedCorrectly(ExploreSection section, bool isVisible)
    {
        // Arrange
        switch (section)
        {
            case ExploreSection.Explore:
                exploreV2MenuController.isInitialized.Set(true);
                break;
            case ExploreSection.Map:
                exploreV2MenuController.isNavmapInitialized.Set(true);
                break;
            case ExploreSection.Quest:
                exploreV2MenuController.isQuestInitialized.Set(true);
                break;
            case ExploreSection.Settings:
                exploreV2MenuController.isSettingsPanelInitialized.Set(true);
                break;
            case ExploreSection.Backpack:
                exploreV2MenuController.isAvatarEditorInitialized.Set(true);
                DataStore.i.common.isSignUpFlow.Set(false);
                break;
        }

        exploreV2MenuController.currentOpenSection = section;

        // Act
        exploreV2MenuController.SetMenuTargetVisibility(section, isVisible);

        // Assert
        exploreV2MenuView.Received().SetVisible(isVisible);

        if (isVisible)
            Assert.That(exploreV2MenuController.currentSectionIndex.Get(), Is.EqualTo((int)section));

        Assert.AreEqual(isVisible, DataStore.i.exploreV2.isOpen.Get());
        exploreV2Analytics.Received().SendStartMenuSectionVisibility(section, isVisible);
    }

    [TestCase(ExploreSection.Quest, true)]
    [TestCase(ExploreSection.Quest,false)]
    [TestCase(ExploreSection.Map, true)]
    [TestCase(ExploreSection.Map,false)]
    [TestCase(ExploreSection.Backpack, true)]
    [TestCase(ExploreSection.Backpack,false)]
    [TestCase(ExploreSection.Settings, true)]
    [TestCase(ExploreSection.Settings,false)]
    public void RaiseIsSectionInitializedChangedCorrectly(ExploreSection section, bool isVisible)
    {
        // Act
        exploreV2MenuController.SectionInitializedChanged(section, isVisible);

        // Assert
        exploreV2MenuView.Received().SetSectionActive(section, isVisible);
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

    [TestCase(ExploreSection.Backpack)]
    [TestCase(ExploreSection.Explore)]
    [TestCase(ExploreSection.Map)]
    [TestCase(ExploreSection.Quest)]
    [TestCase(ExploreSection.Settings)]
    public void GoToSectionCorrectly(ExploreSection section)
    {
        // Arrange
        if (section == ExploreSection.Backpack)
            DataStore.i.exploreV2.currentSectionIndex.Set((int)ExploreSection.Backpack);

        // Act
        exploreV2MenuController.SetMenuTargetVisibility(section, true);

        // Assert
        Assert.AreEqual((int)section, DataStore.i.exploreV2.currentSectionIndex.Get());
    }

    [TestCase(ExploreSection.Explore)]
    [TestCase(ExploreSection.Backpack)]
    [TestCase(ExploreSection.Map)]
    [TestCase(ExploreSection.Quest)]
    [TestCase(ExploreSection.Settings)]
    public void ShouldChangeVisibilityVarsOnViewSectionOpen(ExploreSection sectionId)
    {
        // Arrange
        if (sectionId == ExploreSection.Explore)
            exploreV2MenuController.currentOpenSection = ExploreSection.Backpack ;

        // Act
        exploreV2MenuView.OnSectionOpen += Raise.Event<Action<ExploreSection>>(sectionId);

        // Assert
        Assert.AreEqual(sectionId, exploreV2MenuController.currentOpenSection);

        foreach (KeyValuePair<BaseVariable<bool>, ExploreSection> sectionVisiblityVar in exploreV2MenuController.sectionsByVisibilityVar)
            Assert.IsTrue(sectionVisiblityVar.Key.Get() == (sectionId == sectionVisiblityVar.Value));

        // would be nice to implement: exploreV2MenuView.ReceivedWithAnyArgs(1).GoToSection(default); exploreV2MenuView.Received(1).GoToSection(sectionId);
    }

    [Test]
    public void UpdateRealmInfoCorrectly()
    {
        // Arrange
        ExploreV2ComponentRealmsController realmsController = new ExploreV2ComponentRealmsController(DataStore.i.realm, exploreV2MenuView);

        const string TEST_REALM_NAME = "TestName";
        DataStore.i.realm.playerRealm.Set(new CurrentRealmModel
        {
            serverName = TEST_REALM_NAME,
            layer = null,
        });

        const int TEST_USERS_COUNT = 100;
        List<RealmModel> testRealmList = new List<RealmModel>
        {
            new()
            {
                serverName = TEST_REALM_NAME,
                layer = null,
                usersCount = TEST_USERS_COUNT,
            },
        };
        DataStore.i.realm.realmsInfo.Set(testRealmList.ToArray());

        // Act
        realmsController.UpdateRealmInfo(TEST_REALM_NAME);

        // Assert
        exploreV2MenuView.currentRealmViewer.Received().SetRealm(TEST_REALM_NAME);
        exploreV2MenuView.currentRealmSelectorModal.Received().SetCurrentRealm(TEST_REALM_NAME);
        exploreV2MenuView.currentRealmViewer.Received().SetNumberOfUsers(TEST_USERS_COUNT);
    }

    [Test]
    public void UpdateAvailableRealmsInfoCorrectly()
    {
        // Arrange
        ExploreV2ComponentRealmsController realmsController = new ExploreV2ComponentRealmsController(DataStore.i.realm, exploreV2MenuView);

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
        realmsController.UpdateAvailableRealmsInfo(testRealms);

        // Assert
        Assert.AreEqual(3, realmsController.currentAvailableRealms.Count);
        exploreV2MenuView.currentRealmSelectorModal.Received().SetAvailableRealms(realmsController.currentAvailableRealms);
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
