using DCL;
using DCL.Social.Friends;
using ExploreV2Analytics;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using MainScripts.DCL.Controllers.HotScenes;
using static MainScripts.DCL.Controllers.HotScenes.IHotScenesController;

public class HighlightsSubSectionComponentControllerTests
{
    private HighlightsSubSectionComponentController highlightsSubSectionComponentController;
    private IHighlightsSubSectionComponentView highlightsSubSectionComponentView;
    private IPlacesAPIController placesAPIController;
    private IEventsAPIController eventsAPIController;
    private IFriendsController friendsController;
    private IExploreV2Analytics exploreV2Analytics;

    [SetUp]
    public void SetUp()
    {
        highlightsSubSectionComponentView = Substitute.For<IHighlightsSubSectionComponentView>();
        placesAPIController = Substitute.For<IPlacesAPIController>();
        eventsAPIController = Substitute.For<IEventsAPIController>();
        friendsController = Substitute.For<IFriendsController>();
        exploreV2Analytics = Substitute.For<IExploreV2Analytics>();
        highlightsSubSectionComponentController = new HighlightsSubSectionComponentController(
            highlightsSubSectionComponentView,
            placesAPIController,
            eventsAPIController,
            friendsController,
            exploreV2Analytics,
            DataStore.i);
    }

    [TearDown]
    public void TearDown() { highlightsSubSectionComponentController.Dispose(); }

    [Test]
    public void InitializeCorrectly()
    {
        // Assert
        Assert.AreEqual(highlightsSubSectionComponentView, highlightsSubSectionComponentController.view);
        Assert.AreEqual(placesAPIController, highlightsSubSectionComponentController.placesAPIApiController);
        Assert.AreEqual(eventsAPIController, highlightsSubSectionComponentController.eventsAPIApiController);
        Assert.IsNotNull(highlightsSubSectionComponentController.friendsTrackerController);
    }

    [Test]
    public void DoFirstLoadingCorrectly()
    {
        // Arrange
        highlightsSubSectionComponentController.cardsReloader.firstLoading = true;

        // Act
        highlightsSubSectionComponentController.RequestAllPlacesAndEvents();

        // Assert
        highlightsSubSectionComponentView.Received().RestartScrollViewPosition();
        highlightsSubSectionComponentView.Received().SetAllAsLoading();
        Assert.IsFalse(highlightsSubSectionComponentController.cardsReloader.reloadSubSection);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void RaiseOnExploreV2OpenCorrectly(bool isOpen)
    {
        // Arrange
        highlightsSubSectionComponentController.cardsReloader.reloadSubSection = false;

        // Act
        highlightsSubSectionComponentController.cardsReloader.OnExploreV2Open(isOpen, false);

        // Assert
        if (isOpen)
            Assert.IsFalse(highlightsSubSectionComponentController.cardsReloader.reloadSubSection);
        else
            Assert.IsTrue(highlightsSubSectionComponentController.cardsReloader.reloadSubSection);
    }

    [Test]
    public void RequestAllPlacesCorrectly()
    {
        // Arrange
        highlightsSubSectionComponentController.cardsReloader.reloadSubSection = true;
        highlightsSubSectionComponentController.cardsReloader.lastTimeAPIChecked = Time.realtimeSinceStartup - PlacesAndEventsSectionComponentController.MIN_TIME_TO_CHECK_API;
        DataStore.i.exploreV2.isInShowAnimationTransiton.Set(false);

        // Act
        highlightsSubSectionComponentController.RequestAllPlacesAndEvents();

        // Assert
        highlightsSubSectionComponentView.Received().RestartScrollViewPosition();
        highlightsSubSectionComponentView.Received().SetAllAsLoading();
        placesAPIController.Received().GetAllPlaces(Arg.Any<Action<List<HotSceneInfo>>>());
        Assert.IsFalse(highlightsSubSectionComponentController.cardsReloader.reloadSubSection);
    }

    [Test]
    public void RequestAllPlacesAndEventsFromAPICorrectly()
    {
        // Act
        highlightsSubSectionComponentController.RequestAllFromAPI();

        // Assert
        placesAPIController.Received().GetAllPlaces(Arg.Any<Action<List<HotSceneInfo>>>());
    }

    [Test]
    public void RaiseOnRequestedPlacesAndEventsUpdatedCorrectly()
    {
        // Arrange
        highlightsSubSectionComponentController.placesFromAPI = ExplorePlacesTestHelpers.CreateTestPlacesFromApi(2);
        highlightsSubSectionComponentController.eventsFromAPI = ExploreEventsTestHelpers.CreateTestEventsFromApi(2);

        // Act
        highlightsSubSectionComponentController.OnRequestedPlacesAndEventsUpdated();

        // Assert
        highlightsSubSectionComponentView.Received().SetTrendingPlacesAndEvents(Arg.Any<List<PlaceCardComponentModel>>(), Arg.Any<List<EventCardComponentModel>>());
        highlightsSubSectionComponentView.Received().SetFeaturedPlaces(Arg.Any<List<PlaceCardComponentModel>>());
        highlightsSubSectionComponentView.Received().SetLiveEvents(Arg.Any<List<EventCardComponentModel>>());
    }

    [Test]
    public void LoadPromotedPlacesCorrectly()
    {
        // Arrange
        highlightsSubSectionComponentController.placesFromAPI = ExplorePlacesTestHelpers.CreateTestPlacesFromApi(2);
        highlightsSubSectionComponentController.eventsFromAPI = ExploreEventsTestHelpers.CreateTestEventsFromApi(2);

        // Act
        List<PlaceCardComponentModel> trendingPlaces = PlacesAndEventsCardsFactory.CreatePlacesCards(highlightsSubSectionComponentController.FilterTrendingPlaces());
        List<EventCardComponentModel> trendingEvents = PlacesAndEventsCardsFactory.CreateEventsCards(highlightsSubSectionComponentController.FilterTrendingEvents(trendingPlaces.Count));
        highlightsSubSectionComponentController.view.SetTrendingPlacesAndEvents(trendingPlaces, trendingEvents);

        // Assert
        highlightsSubSectionComponentView.Received().SetTrendingPlacesAndEvents(Arg.Any<List<PlaceCardComponentModel>>(), Arg.Any<List<EventCardComponentModel>>());
    }

    [Test]
    public void LoadFeaturedPlacesCorrectly()
    {
        // Arrange
        int numberOfPlaces = 2;
        highlightsSubSectionComponentController.placesFromAPI = ExplorePlacesTestHelpers.CreateTestPlacesFromApi(numberOfPlaces);

        // Act
        highlightsSubSectionComponentController.view.SetFeaturedPlaces(PlacesAndEventsCardsFactory.CreatePlacesCards(highlightsSubSectionComponentController.FilterFeaturedPlaces()));

        // Assert
        highlightsSubSectionComponentView.Received().SetFeaturedPlaces(Arg.Any<List<PlaceCardComponentModel>>());
    }

    [Test]
    public void LoadTrendingEventsCorrectly()
    {
        // Arrange
        int numberOfEvents = 2;
        highlightsSubSectionComponentController.eventsFromAPI = ExploreEventsTestHelpers.CreateTestEventsFromApi(numberOfEvents);

        // Act
        highlightsSubSectionComponentController.view.SetLiveEvents(PlacesAndEventsCardsFactory.CreateEventsCards(highlightsSubSectionComponentController.FilterLiveEvents()));

        // Assert
        highlightsSubSectionComponentView.Received().SetLiveEvents(Arg.Any<List<EventCardComponentModel>>());
    }

    [Test]
    public void ShowPlaceDetailedInfoCorrectly()
    {
        // Arrange
        PlaceCardComponentModel testPlaceCardModel = new PlaceCardComponentModel();
        testPlaceCardModel.hotSceneInfo = new HotSceneInfo();

        // Act
        highlightsSubSectionComponentController.ShowPlaceDetailedInfo(testPlaceCardModel);

        // Assert
        highlightsSubSectionComponentView.Received().ShowPlaceModal(testPlaceCardModel);
        exploreV2Analytics.Received().SendClickOnPlaceInfo(testPlaceCardModel.hotSceneInfo.id, testPlaceCardModel.placeName);
    }

    [Test]
    public void JumpInToPlaceCorrectly()
    {
        // Arrange
        bool exploreClosed = false;
        highlightsSubSectionComponentController.OnCloseExploreV2 += () => exploreClosed = true;
        HotSceneInfo testPlaceFromAPI = ExplorePlacesTestHelpers.CreateTestHotSceneInfo("1");

        // Act
        highlightsSubSectionComponentController.JumpInToPlace(testPlaceFromAPI);

        // Assert
        highlightsSubSectionComponentView.Received().HidePlaceModal();
        Assert.IsTrue(exploreClosed);
        exploreV2Analytics.Received().SendPlaceTeleport(testPlaceFromAPI.id, testPlaceFromAPI.name, testPlaceFromAPI.baseCoords);
    }

    [Test]
    public void ShowEventDetailedInfoCorrectly()
    {
        // Arrange
        EventCardComponentModel testEventCardModel = new EventCardComponentModel();

        // Act
        highlightsSubSectionComponentController.ShowEventDetailedInfo(testEventCardModel);

        // Assert
        highlightsSubSectionComponentView.Received().ShowEventModal(testEventCardModel);
        exploreV2Analytics.Received().SendClickOnEventInfo(testEventCardModel.eventId, testEventCardModel.eventName);
    }

    [Test]
    public void JumpInToEventCorrectly()
    {
        // Arrange
        bool exploreClosed = false;
        highlightsSubSectionComponentController.OnCloseExploreV2 += () => exploreClosed = true;
        EventFromAPIModel testEventFromAPI = ExploreEventsTestHelpers.CreateTestEvent("1");

        // Act
        highlightsSubSectionComponentController.JumpInToEvent(testEventFromAPI);

        // Assert
        highlightsSubSectionComponentView.Received().HideEventModal();
        Assert.IsTrue(exploreClosed);
        exploreV2Analytics.Received().SendEventTeleport(testEventFromAPI.id, testEventFromAPI.name, new Vector2Int(testEventFromAPI.coordinates[0], testEventFromAPI.coordinates[1]));
    }

    [Test]
    public void GoToEventsSubSectionCorrectly()
    {
        // Arrange
        bool goToEventsSubSectionClicked = false;
        highlightsSubSectionComponentController.OnGoToEventsSubSection += () => goToEventsSubSectionClicked = true;

        // Act
        highlightsSubSectionComponentController.GoToEventsSubSection();

        // Assert
        Assert.IsTrue(goToEventsSubSectionClicked);
    }
}
