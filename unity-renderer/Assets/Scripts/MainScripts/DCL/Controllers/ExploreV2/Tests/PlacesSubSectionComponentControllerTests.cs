using DCL;
using ExploreV2Analytics;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using static HotScenesController;

public class PlacesSubSectionComponentControllerTests
{
    private PlacesSubSectionComponentController placesSubSectionComponentController;
    private IPlacesSubSectionComponentView placesSubSectionComponentView;
    private IPlacesAPIController placesAPIController;
    private IFriendsController friendsController;
    private IExploreV2Analytics exploreV2Analytics;

    [SetUp]
    public void SetUp()
    {
        placesSubSectionComponentView = Substitute.For<IPlacesSubSectionComponentView>();
        placesAPIController = Substitute.For<IPlacesAPIController>();
        friendsController = Substitute.For<IFriendsController>();
        exploreV2Analytics = Substitute.For<IExploreV2Analytics>();
        placesSubSectionComponentController = new PlacesSubSectionComponentController(placesSubSectionComponentView, placesAPIController, friendsController, exploreV2Analytics, DataStore.i);
    }

    [TearDown]
    public void TearDown() { placesSubSectionComponentController.Dispose(); }

    [Test]
    public void InitializeCorrectly()
    {
        // Assert
        Assert.AreEqual(placesSubSectionComponentView, placesSubSectionComponentController.view);
        Assert.AreEqual(placesAPIController, placesSubSectionComponentController.placesAPIApiController);
        Assert.IsNotNull(placesSubSectionComponentController.friendsTrackerController);
    }

    [Test]
    public void DoFirstLoadingCorrectly()
    {
        // Arrange
        placesSubSectionComponentController.reloadPlaces = true;

        // Act
        placesSubSectionComponentController.FirstLoading();

        // Assert
        placesSubSectionComponentView.Received().RestartScrollViewPosition();
        placesSubSectionComponentView.Received().SetPlacesAsLoading(true);
        placesSubSectionComponentView.Received().SetShowMorePlacesButtonActive(false);
        Assert.IsFalse(placesSubSectionComponentController.reloadPlaces);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void RaiseOnExploreV2OpenCorrectly(bool isOpen)
    {
        // Arrange
        placesSubSectionComponentController.reloadPlaces = false;

        // Act
        placesSubSectionComponentController.OnExploreV2Open(isOpen, false);

        // Assert
        if (isOpen)
            Assert.IsFalse(placesSubSectionComponentController.reloadPlaces);
        else
            Assert.IsTrue(placesSubSectionComponentController.reloadPlaces);
    }

    [Test]
    public void RequestAllPlacesCorrectly()
    {
        // Arrange
        placesSubSectionComponentController.currentPlacesShowed = -1;
        placesSubSectionComponentController.reloadPlaces = true;
        placesSubSectionComponentController.lastTimeAPIChecked = Time.realtimeSinceStartup - PlacesAndEventsSectionComponentController.MIN_TIME_TO_CHECK_API;
        DataStore.i.exploreV2.isInShowAnimationTransiton.Set(false);

        // Act
        placesSubSectionComponentController.RequestAllPlaces();

        // Assert
        Assert.AreEqual(placesSubSectionComponentView.currentPlacesPerRow * PlacesSubSectionComponentController.INITIAL_NUMBER_OF_ROWS, placesSubSectionComponentController.currentPlacesShowed);
        placesSubSectionComponentView.Received().RestartScrollViewPosition();
        placesSubSectionComponentView.Received().SetPlacesAsLoading(true);
        placesSubSectionComponentView.Received().SetShowMorePlacesButtonActive(false);
        placesAPIController.Received().GetAllPlaces(Arg.Any<Action<List<HotSceneInfo>>>());
        Assert.IsFalse(placesSubSectionComponentController.reloadPlaces);
    }

    [Test]
    public void RequestAllPlacesFromAPICorrectly()
    {
        // Act
        placesSubSectionComponentController.RequestAllPlacesFromAPI();

        // Assert
        placesAPIController.Received().GetAllPlaces(Arg.Any<Action<List<HotSceneInfo>>>());
    }

    [Test]
    public void LoadPlacesCorrectly()
    {
        // Arrange
        int numberOfPlaces = 2;
        placesSubSectionComponentController.placesFromAPI = ExplorePlacesTestHelpers.CreateTestPlacesFromApi(numberOfPlaces);

        // Act
        placesSubSectionComponentController.LoadPlaces(placesSubSectionComponentController.placesFromAPI);

        // Assert
        placesSubSectionComponentView.Received().SetPlaces(Arg.Any<List<PlaceCardComponentModel>>());
        placesSubSectionComponentView.Received().SetShowMorePlacesButtonActive(placesSubSectionComponentController.currentPlacesShowed < numberOfPlaces);
    }

    [Test]
    [TestCase(2)]
    [TestCase(10)]
    public void LoaShowMorePlacesCorrectly(int numberOfPlaces)
    {
        // Act
        placesSubSectionComponentController.ShowMorePlaces();

        // Assert
        placesSubSectionComponentView.Received().SetShowMorePlacesButtonActive(Arg.Any<bool>());
    }

    [Test]
    public void ShowPlaceDetailedInfoCorrectly()
    {
        // Arrange
        PlaceCardComponentModel testPlaceCardModel = new PlaceCardComponentModel();
        testPlaceCardModel.hotSceneInfo = new HotSceneInfo();

        // Act
        placesSubSectionComponentController.ShowPlaceDetailedInfo(testPlaceCardModel);

        // Assert
        placesSubSectionComponentView.Received().ShowPlaceModal(testPlaceCardModel);
        exploreV2Analytics.Received().SendClickOnPlaceInfo(testPlaceCardModel.hotSceneInfo.id, testPlaceCardModel.placeName);
    }

    [Test]
    public void JumpInToPlaceCorrectly()
    {
        // Arrange
        bool exploreClosed = false;
        placesSubSectionComponentController.OnCloseExploreV2 += () => exploreClosed = true;
        HotSceneInfo testPlaceFromAPI = ExplorePlacesTestHelpers.CreateTestHotSceneInfo("1");

        // Act
        placesSubSectionComponentController.JumpInToPlace(testPlaceFromAPI);

        // Assert
        placesSubSectionComponentView.Received().HidePlaceModal();
        Assert.IsTrue(exploreClosed);
        exploreV2Analytics.Received().SendPlaceTeleport(testPlaceFromAPI.id, testPlaceFromAPI.name, testPlaceFromAPI.baseCoords);
    }
}