using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using DCL.Social.Friends;
using DCLServices.PlacesAPIService;
using DCLServices.WorldsAPIService;
using ExploreV2Analytics;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using MainScripts.DCL.Controllers.HotScenes;
using System.Threading;

public class WorldsSubSectionComponentControllerTests
{
    private WorldsSubSectionComponentController worldsSubSectionComponentController;
    private IWorldsSubSectionComponentView worldsSubSectionComponentView;
    private IPlacesAPIService placesAPIService;
    private IWorldsAPIService worldsAPIService;
    private IFriendsController friendsController;
    private IExploreV2Analytics exploreV2Analytics;

    [SetUp]
    public void SetUp()
    {
        worldsSubSectionComponentView = Substitute.For<IWorldsSubSectionComponentView>();
        placesAPIService = Substitute.For<IPlacesAPIService>();
        worldsAPIService = Substitute.For<IWorldsAPIService>();
        worldsAPIService.GetWorlds(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(),Arg.Any<CancellationToken>()).Returns( new UniTask<(IReadOnlyList<WorldsResponse.WorldInfo> worlds, int total)>((new List<WorldsResponse.WorldInfo>(), 0)));
        friendsController = Substitute.For<IFriendsController>();
        exploreV2Analytics = Substitute.For<IExploreV2Analytics>();
        worldsSubSectionComponentController = new WorldsSubSectionComponentController(
            worldsSubSectionComponentView,
            placesAPIService,
            worldsAPIService,
            friendsController,
            exploreV2Analytics,
            Substitute.For<IPlacesAnalytics>(),
            DataStore.i,
            Substitute.For<IUserProfileBridge>());
    }

    [TearDown]
    public void TearDown() { worldsSubSectionComponentController.Dispose(); }

    [Test]
    public void InitializeCorrectly()
    {
        // Assert
        Assert.AreEqual(worldsSubSectionComponentView, worldsSubSectionComponentController.view);
        Assert.AreEqual(worldsAPIService, worldsSubSectionComponentController.worldsAPIService);
        Assert.IsNotNull(worldsSubSectionComponentController.friendsTrackerController);
    }

    [Test]
    public void DoFirstLoadingCorrectly()
    {
        // Arrange
        worldsSubSectionComponentController.cardsReloader.firstLoading = true;

        // Act
        worldsSubSectionComponentController.RequestAllWorlds();

        // Assert
        worldsSubSectionComponentView.Received().RestartScrollViewPosition();
        worldsSubSectionComponentView.Received().SetAllAsLoading();
        Assert.IsFalse(worldsSubSectionComponentController.cardsReloader.reloadSubSection);
        worldsSubSectionComponentView.Received().SetShowMoreButtonActive(false);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void RaiseOnExploreV2OpenCorrectly(bool isOpen)
    {
        // Arrange
        worldsSubSectionComponentController.cardsReloader.reloadSubSection = false;

        // Act
        worldsSubSectionComponentController.cardsReloader.OnExploreV2Open(isOpen, false);

        // Assert
        Assert.That(worldsSubSectionComponentController.cardsReloader.reloadSubSection, Is.EqualTo(!isOpen));
    }

    [Test]
    public void RequestWorldsCorrectly()
    {
        // Arrange
        worldsSubSectionComponentController.availableUISlots = -1;
        worldsSubSectionComponentController.cardsReloader.reloadSubSection = true;
        worldsSubSectionComponentController.cardsReloader.lastTimeAPIChecked = Time.realtimeSinceStartup - PlacesAndEventsSectionComponentController.MIN_TIME_TO_CHECK_API;
        DataStore.i.exploreV2.isInShowAnimationTransiton.Set(false);

        // Act
        worldsSubSectionComponentController.RequestAllWorlds();

        // Assert
        Assert.AreEqual(worldsSubSectionComponentView.currentWorldsPerRow * WorldsSubSectionComponentController.INITIAL_NUMBER_OF_ROWS, worldsSubSectionComponentController.availableUISlots);
        worldsSubSectionComponentView.Received().RestartScrollViewPosition();
        worldsSubSectionComponentView.Received().SetAllAsLoading();
        worldsSubSectionComponentView.Received().SetShowMoreButtonActive(false);
        worldsAPIService.Received().GetWorlds(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(),Arg.Any<CancellationToken>());
        Assert.IsFalse(worldsSubSectionComponentController.cardsReloader.reloadSubSection);
    }

    [Test]
    public void RequestAllWorldsFromAPICorrectly()
    {
        // Act
        worldsSubSectionComponentController.RequestAllFromAPI();

        // Assert
        worldsAPIService.Received().GetWorlds(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(),Arg.Any<CancellationToken>());
    }

    [Test]
    public void LoadWorldsCorrectly()
    {
        // Arrange
        int numberOfWorlds = 2;
        worldsSubSectionComponentController.worldsFromAPI.Clear();
        worldsSubSectionComponentController.worldsFromAPI.AddRange(ExplorePlacesTestHelpers.CreateTestWorldsFromApi(numberOfWorlds));

        // Act
        worldsSubSectionComponentController.view.SetWorlds(PlacesAndEventsCardsFactory.ConvertWorldsResponseToModel(worldsSubSectionComponentController.worldsFromAPI, worldsSubSectionComponentController.availableUISlots));

        // Assert
        worldsSubSectionComponentView.Received().SetWorlds(Arg.Any<List<PlaceCardComponentModel>>());
    }

    [Test]
    [TestCase(2)]
    [TestCase(10)]
    public void LoadShowMoreWorldsCorrectly(int numberOfWorlds)
    {
        // Act
        worldsSubSectionComponentController.ShowMoreWorlds();
        // Assert
        worldsAPIService.Received().GetWorlds(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(),Arg.Any<CancellationToken>());
    }

    [Test]
    [Ignore("TODO: Fix this test")]
    public void ShowWorldDetailedInfoCorrectly()
    {
        // Arrange
        PlaceCardComponentModel testWorldsCard = new PlaceCardComponentModel
            {
                placeInfo = new IHotScenesController.PlaceInfo()
                {
                    base_position = "10,10",
                    title = "Test world"
                },
            };

        // Act
        worldsSubSectionComponentController.ShowWorldDetailedInfo(testWorldsCard);

        // Assert
        worldsSubSectionComponentView.Received().ShowWorldModal(testWorldsCard);
        exploreV2Analytics.Received().SendClickOnPlaceInfo(testWorldsCard.placeInfo.id, testWorldsCard.placeName);
    }

    [Test]
    [Ignore("TODO: Fix this test")]
    public void JumpInToWorldCorrectly()
    {
        // Arrange
        bool exploreClosed = false;
        worldsSubSectionComponentController.OnCloseExploreV2 += () => exploreClosed = true;
        IHotScenesController.PlaceInfo testPlaceFromAPI = ExplorePlacesTestHelpers.CreateTestHotSceneInfo("1");

        // Act
        worldsSubSectionComponentController.OnJumpInToWorld(testPlaceFromAPI);

        // Assert
        worldsSubSectionComponentView.Received().HideWorldModal();
        Assert.IsTrue(exploreClosed);
        exploreV2Analytics.Received().SendPlaceTeleport(testPlaceFromAPI.id, testPlaceFromAPI.title, Utils.ConvertStringToVector(testPlaceFromAPI.base_position));
    }
}
