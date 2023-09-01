using Cysharp.Threading.Tasks;
using DCL;
using DCLServices.PlacesAPIService;
using DCLServices.WorldsAPIService;
using ExploreV2Analytics;
using MainScripts.DCL.Controllers.HotScenes;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class EventsSubSectionComponentControllerTests
{
    private EventsSubSectionComponentController eventsSubSectionComponentController;
    private IEventsSubSectionComponentView eventsSubSectionComponentView;
    private IEventsAPIController eventsAPIController;
    private IExploreV2Analytics exploreV2Analytics;
    private IUserProfileBridge userProfileBridge;
    private IPlacesAPIService placesAPIService;
    private IWorldsAPIService worldsAPIService;

    [SetUp]
    public void SetUp()
    {
        // This is need to sue the TeleportController
        ServiceLocator serviceLocator = ServiceLocatorTestFactory.CreateMocked();
        DCL.Environment.Setup(serviceLocator);

        eventsSubSectionComponentView = Substitute.For<IEventsSubSectionComponentView>();
        eventsSubSectionComponentView.SelectedEventType.Returns(EventsType.Upcoming);
        eventsSubSectionComponentView.SelectedFrequency.Returns("all");
        eventsSubSectionComponentView.SelectedCategory.Returns("all");
        eventsSubSectionComponentView.SelectedLowTime.Returns(0);
        eventsSubSectionComponentView.SelectedHighTime.Returns(48);
        eventsAPIController = Substitute.For<IEventsAPIController>();
        exploreV2Analytics = Substitute.For<IExploreV2Analytics>();
        userProfileBridge = Substitute.For<IUserProfileBridge>();
        placesAPIService = Substitute.For<IPlacesAPIService>();
        placesAPIService
           .Configure()
           .GetPlacesByCoordsList(Arg.Any<IEnumerable<Vector2Int>>(), Arg.Any<CancellationToken>())
           .Returns(new UniTask<List<IHotScenesController.PlaceInfo>>(new List<IHotScenesController.PlaceInfo>()));
        worldsAPIService = Substitute.For<IWorldsAPIService>();
        worldsAPIService
           .Configure()
           .GetWorldsByNamesList(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
           .Returns(new UniTask<List<WorldsResponse.WorldInfo>>(new List<WorldsResponse.WorldInfo>()));
        var ownUserProfile = ScriptableObject.CreateInstance<UserProfile>();
        ownUserProfile.UpdateData(new UserProfileModel
        {
            userId = "ownId",
            hasConnectedWeb3 = true
        });
        userProfileBridge.GetOwn().Returns(ownUserProfile);
        eventsSubSectionComponentController = new EventsSubSectionComponentController(
            eventsSubSectionComponentView,
            eventsAPIController,
            exploreV2Analytics,
            DataStore.i,
            userProfileBridge,
            placesAPIService,
            worldsAPIService);
    }

    [TearDown]
    public void TearDown() { eventsSubSectionComponentController.Dispose(); }

    [Test]
    public void InitializeCorrectly()
    {
        // Assert
        Assert.AreEqual(eventsSubSectionComponentView, eventsSubSectionComponentController.view);
        Assert.AreEqual(eventsAPIController, eventsSubSectionComponentController.eventsAPIApiController);
    }

    [Test]
    public void DoFirstLoadingCorrectly()
    {
        // Arrange
        eventsSubSectionComponentController.cardsReloader.firstLoading = true;

        // Act
        eventsSubSectionComponentController.RequestAllEvents();

        // Assert
        eventsSubSectionComponentView.Received().RestartScrollViewPosition();
        eventsSubSectionComponentView.Received().SetAllAsLoading();
        Assert.IsFalse(eventsSubSectionComponentController.cardsReloader.reloadSubSection);
        eventsSubSectionComponentView.Received().SetShowMoreEventsButtonActive(false);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void RaiseOnExploreV2OpenCorrectly(bool isOpen)
    {
        // Arrange
        eventsSubSectionComponentController.cardsReloader.reloadSubSection = false;

        // Act
        eventsSubSectionComponentController.cardsReloader.OnExploreV2Open(isOpen, false);

        // Assert
        Assert.That(eventsSubSectionComponentController.cardsReloader.reloadSubSection, Is.EqualTo(!isOpen));
    }

    [Test]
    public void RequestAllEventsCorrectly()
    {
        // Arrange
        eventsSubSectionComponentController.availableUISlots = -1;
        eventsSubSectionComponentController.cardsReloader.reloadSubSection = true;
        eventsSubSectionComponentController.cardsReloader.lastTimeAPIChecked = Time.realtimeSinceStartup - PlacesAndEventsSectionComponentController.MIN_TIME_TO_CHECK_API;
        DataStore.i.exploreV2.isInShowAnimationTransiton.Set(false);

        // Act
        eventsSubSectionComponentController.RequestAllEvents();

        // Assert
        Assert.AreEqual(eventsSubSectionComponentView.currentEventsPerRow * EventsSubSectionComponentController.INITIAL_NUMBER_OF_ROWS, eventsSubSectionComponentController.availableUISlots);
        eventsSubSectionComponentView.Received().RestartScrollViewPosition();
        eventsSubSectionComponentView.Received().SetAllAsLoading();
        eventsSubSectionComponentView.Received().SetShowMoreEventsButtonActive(false);
        eventsAPIController.Received().GetAllEvents(Arg.Any<Action<List<EventFromAPIModel>>>(), Arg.Any<Action<string>>());
        Assert.IsFalse(eventsSubSectionComponentController.cardsReloader.reloadSubSection);
    }

    [Test]
    public void RequestAllEventsFromAPICorrectly()
    {
        // Act
        eventsSubSectionComponentController.RequestAllFromAPI();

        // Assert
        eventsAPIController.Received().GetAllEvents(Arg.Any<Action<List<EventFromAPIModel>>>(), Arg.Any<Action<string>>());
    }

    [Test]
    public void RaiseOnRequestedEventsUpdatedCorrectly()
    {
        // Arrange
        int numberOfEvents = 2;
        eventsSubSectionComponentController.eventsFromAPI = ExploreEventsTestHelpers.CreateTestEventsFromApi(numberOfEvents);

        List<IHotScenesController.PlaceInfo> testPlaces = new ()
        {
            new IHotScenesController.PlaceInfo
            {
                id = "testId1",
                title = "place_1",
                Positions = new []{ new Vector2Int(1,1) },
            },
            new IHotScenesController.PlaceInfo
            {
                id = "testId2",
                title = "place_2",
                Positions = new []{ new Vector2Int(eventsSubSectionComponentController.eventsFromAPI[0].coordinates[0],eventsSubSectionComponentController.eventsFromAPI[0].coordinates[1]) },
            },
            new IHotScenesController.PlaceInfo
            {
                id = "testId3",
                title = "place_3",
                Positions = new []{ new Vector2Int(3,3) },
            },
        };

        placesAPIService.Configure()
                        .GetPlacesByCoordsList(Arg.Any<IEnumerable<Vector2Int>>(), Arg.Any<CancellationToken>())
                        .Returns(new UniTask<List<IHotScenesController.PlaceInfo>>(testPlaces));

        // Act
        eventsSubSectionComponentController.OnRequestedEventsUpdated(eventsSubSectionComponentController.eventsFromAPI);

        // Assert
        foreach (var testEvent in eventsSubSectionComponentController.eventsFromAPI)
            Assert.AreEqual("place_2", testEvent.scene_name);

        eventsSubSectionComponentView.Received().SetFeaturedEvents(Arg.Any<List<EventCardComponentModel>>());
        eventsSubSectionComponentView.Received().SetEvents(Arg.Any<List<EventCardComponentModel>>());
        eventsSubSectionComponentView.Received().SetShowMoreEventsButtonActive(eventsSubSectionComponentController.availableUISlots < numberOfEvents);
    }

    [Test]
    public void LoadFeaturedEventsCorrectly()
    {
        // Arrange
        int numberOfEvents = 2;
        eventsSubSectionComponentController.eventsFromAPI = ExploreEventsTestHelpers.CreateTestEventsFromApi(numberOfEvents);

        // Act
        eventsSubSectionComponentView.SetFeaturedEvents(PlacesAndEventsCardsFactory.CreateEventsCards(eventsSubSectionComponentController.FilterFeaturedEvents()));

        // Assert
        eventsSubSectionComponentView.Received().SetFeaturedEvents(Arg.Any<List<EventCardComponentModel>>());
    }

    [Test]
    public void LoadUpcomingEventsCorrectly()
    {
        // Arrange
        int numberOfEvents = 2;
        eventsSubSectionComponentController.eventsFromAPI = ExploreEventsTestHelpers.CreateTestEventsFromApi(numberOfEvents);

        // Act
        eventsSubSectionComponentView.SetEvents(PlacesAndEventsCardsFactory.CreateEventsCards(eventsSubSectionComponentController.FilterUpcomingEvents()));

        // Assert
        eventsSubSectionComponentView.Received().SetEvents(Arg.Any<List<EventCardComponentModel>>());
    }

    [Test]
    [TestCase(2)]
    [TestCase(10)]
    public void LoaShowMoreUpcomingEventsCorrectly(int numberOfPlaces)
    {
        // Act
        eventsSubSectionComponentController.ShowMoreEvents();

        // Assert
        eventsSubSectionComponentView.Received().SetShowMoreEventsButtonActive(Arg.Any<bool>());
    }

    [Test]
    public void ShowEventDetailedInfoCorrectly()
    {
        // Arrange
        EventCardComponentModel testEventCardModel = new EventCardComponentModel();

        // Act
        eventsSubSectionComponentController.ShowEventDetailedInfo(testEventCardModel);

        // Assert
        eventsSubSectionComponentView.Received().ShowEventModal(testEventCardModel);
        exploreV2Analytics.Received().SendClickOnEventInfo(testEventCardModel.eventId, testEventCardModel.eventName);
    }

    [Test]
    public void JumpInToEventCorrectly()
    {
        // Arrange
        var exploreClosed = false;
        eventsSubSectionComponentController.OnCloseExploreV2 += () => exploreClosed = true;
        EventFromAPIModel testEventFromAPI = ExploreEventsTestHelpers.CreateTestEvent("1");

        // Act
        eventsSubSectionComponentController.OnJumpInToEvent(testEventFromAPI);

        // Assert
        eventsSubSectionComponentView.Received().HideEventModal();
        Assert.IsTrue(exploreClosed);
        exploreV2Analytics.Received().SendEventTeleport(testEventFromAPI.id, testEventFromAPI.name, new Vector2Int(testEventFromAPI.coordinates[0], testEventFromAPI.coordinates[1]));
    }
}
