using DCL;
using DCL.Helpers;
using DCL.Social.Friends;
using ExploreV2Analytics;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using MainScripts.DCL.Controllers.HotScenes;
using System.Threading;

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
        placesSubSectionComponentController.cardsReloader.firstLoading = true;

        // Act
        placesSubSectionComponentController.RequestAllPlaces();

        // Assert
        placesSubSectionComponentView.Received().RestartScrollViewPosition();
        placesSubSectionComponentView.Received().SetAllAsLoading();
        Assert.IsFalse(placesSubSectionComponentController.cardsReloader.reloadSubSection);
        placesSubSectionComponentView.Received().SetShowMoreButtonActive(false);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void RaiseOnExploreV2OpenCorrectly(bool isOpen)
    {
        // Arrange
        placesSubSectionComponentController.cardsReloader.reloadSubSection = false;

        // Act
        placesSubSectionComponentController.cardsReloader.OnExploreV2Open(isOpen, false);

        // Assert
        Assert.That(placesSubSectionComponentController.cardsReloader.reloadSubSection, Is.EqualTo(!isOpen));
    }

    [Test]
    public void RequestAllPlacesCorrectly()
    {
        // Arrange
        placesSubSectionComponentController.availableUISlots = -1;
        placesSubSectionComponentController.cardsReloader.reloadSubSection = true;
        placesSubSectionComponentController.cardsReloader.lastTimeAPIChecked = Time.realtimeSinceStartup - PlacesAndEventsSectionComponentController.MIN_TIME_TO_CHECK_API;
        DataStore.i.exploreV2.isInShowAnimationTransiton.Set(false);

        // Act
        placesSubSectionComponentController.RequestAllPlaces();

        // Assert
        Assert.AreEqual(placesSubSectionComponentView.currentPlacesPerRow * PlacesSubSectionComponentController.INITIAL_NUMBER_OF_ROWS, placesSubSectionComponentController.availableUISlots);
        placesSubSectionComponentView.Received().RestartScrollViewPosition();
        placesSubSectionComponentView.Received().SetAllAsLoading();
        placesSubSectionComponentView.Received().SetShowMoreButtonActive(false);
        placesAPIController.Received().GetAllPlacesFromPlacesAPI(Arg.Any<Action<List<IHotScenesController.PlaceInfo>, int>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        Assert.IsFalse(placesSubSectionComponentController.cardsReloader.reloadSubSection);
    }

    [Test]
    public void RequestAllPlacesFromAPICorrectly()
    {
        // Act
        placesSubSectionComponentController.RequestAllFromAPI();

        // Assert
        placesAPIController.Received().GetAllPlacesFromPlacesAPI(Arg.Any<Action<List<IHotScenesController.PlaceInfo>, int>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public void LoadPlacesCorrectly()
    {
        // Arrange
        int numberOfPlaces = 2;
        placesSubSectionComponentController.placesFromAPI = ExplorePlacesTestHelpers.CreateTestPlacesFromApi(numberOfPlaces);

        // Act
        placesSubSectionComponentController.view.SetPlaces(PlacesAndEventsCardsFactory.ConvertPlaceResponseToModel(
            placesSubSectionComponentController.TakeAllForAvailableSlots(placesSubSectionComponentController.placesFromAPI)));

        // Assert
        placesSubSectionComponentView.Received().SetPlaces(Arg.Any<List<PlaceCardComponentModel>>());
    }

    [Test]
    [TestCase(2)]
    [TestCase(10)]
    public void LoadShowMorePlacesCorrectly(int numberOfPlaces)
    {
        // Act
        placesSubSectionComponentController.ShowMorePlaces();
        // Assert
        placesAPIController.Received().GetAllPlacesFromPlacesAPI(Arg.Any<Action<List<IHotScenesController.PlaceInfo>, int>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public void ShowPlaceDetailedInfoCorrectly()
    {
        // Arrange
        PlaceCardComponentModel testPlaceCardModel = new PlaceCardComponentModel
            {
                placeInfo = new IHotScenesController.PlaceInfo()
                {
                    base_position = "10,10",
                    title = "Test place"
                },
            };

        // Act
        placesSubSectionComponentController.ShowPlaceDetailedInfo(testPlaceCardModel);

        // Assert
        placesSubSectionComponentView.Received().ShowPlaceModal(testPlaceCardModel);
        exploreV2Analytics.Received().SendClickOnPlaceInfo(testPlaceCardModel.placeInfo.id, testPlaceCardModel.placeName);
    }

    [Test]
    public void JumpInToPlaceCorrectly()
    {
        // Arrange
        bool exploreClosed = false;
        placesSubSectionComponentController.OnCloseExploreV2 += () => exploreClosed = true;
        IHotScenesController.PlaceInfo testPlaceFromAPI = ExplorePlacesTestHelpers.CreateTestHotSceneInfo("1");

        // Act
        placesSubSectionComponentController.OnJumpInToPlace(testPlaceFromAPI);

        // Assert
        placesSubSectionComponentView.Received().HidePlaceModal();
        Assert.IsTrue(exploreClosed);
        exploreV2Analytics.Received().SendPlaceTeleport(testPlaceFromAPI.id, testPlaceFromAPI.title, Utils.ConvertStringToVector(testPlaceFromAPI.base_position));
    }
}
