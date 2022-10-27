using DCL;
using ExploreV2Analytics;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class EventsSubSectionComponentControllerTests
{
    private EventsSubSectionComponentController eventsSubSectionComponentController;
    private IEventsSubSectionComponentView eventsSubSectionComponentView;
    private IEventsAPIController eventsAPIController;
    private IExploreV2Analytics exploreV2Analytics;

    [SetUp]
    public void SetUp()
    {
        // This is need to sue the TeleportController
        ServiceLocator serviceLocator = ServiceLocatorTestFactory.CreateMocked();
        DCL.Environment.Setup(serviceLocator);
        
        eventsSubSectionComponentView = Substitute.For<IEventsSubSectionComponentView>();
        eventsAPIController = Substitute.For<IEventsAPIController>();
        exploreV2Analytics = Substitute.For<IExploreV2Analytics>();
        eventsSubSectionComponentController = new EventsSubSectionComponentController(eventsSubSectionComponentView, eventsAPIController, exploreV2Analytics, DataStore.i);
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
        eventsSubSectionComponentController.reloadEvents = true;

        // Act
        eventsSubSectionComponentController.FirstLoading();

        // Assert
        eventsSubSectionComponentView.Received().RestartScrollViewPosition();
        eventsSubSectionComponentView.Received().SetAllEventGroupsAsLoading();
        eventsSubSectionComponentView.Received().SetShowMoreUpcomingEventsButtonActive(false);
        Assert.IsFalse(eventsSubSectionComponentController.reloadEvents);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void RaiseOnExploreV2OpenCorrectly(bool isOpen)
    {
        // Arrange
        eventsSubSectionComponentController.reloadEvents = false;

        // Act
        eventsSubSectionComponentController.OnExploreV2Open(isOpen, false);

        // Assert
        if (isOpen)
            Assert.IsFalse(eventsSubSectionComponentController.reloadEvents);
        else
            Assert.IsTrue(eventsSubSectionComponentController.reloadEvents);
    }

    [Test]
    public void RequestAllEventsCorrectly()
    {
        // Arrange
        eventsSubSectionComponentController.currentUpcomingEventsShowed = -1;
        eventsSubSectionComponentController.reloadEvents = true;
        eventsSubSectionComponentController.lastTimeAPIChecked = Time.realtimeSinceStartup - PlacesAndEventsSectionComponentController.MIN_TIME_TO_CHECK_API;
        DataStore.i.exploreV2.isInShowAnimationTransiton.Set(false);

        // Act
        eventsSubSectionComponentController.RequestAllEvents();

        // Assert
        Assert.AreEqual(eventsSubSectionComponentView.currentUpcomingEventsPerRow * EventsSubSectionComponentController.INITIAL_NUMBER_OF_UPCOMING_ROWS, eventsSubSectionComponentController.currentUpcomingEventsShowed);
        eventsSubSectionComponentView.Received().RestartScrollViewPosition();
        eventsSubSectionComponentView.Received().SetAllEventGroupsAsLoading();
        eventsSubSectionComponentView.Received().SetShowMoreUpcomingEventsButtonActive(false);
        eventsAPIController.Received().GetAllEvents(Arg.Any<Action<List<EventFromAPIModel>>>(), Arg.Any<Action<string>>());
        Assert.IsFalse(eventsSubSectionComponentController.reloadEvents);
    }

    [Test]
    public void RequestAllEventsFromAPICorrectly()
    {
        // Act
        eventsSubSectionComponentController.RequestAllEventsFromAPI();

        // Assert
        eventsAPIController.Received().GetAllEvents(Arg.Any<Action<List<EventFromAPIModel>>>(), Arg.Any<Action<string>>());
    }

    [Test]
    public void RaiseOnRequestedEventsUpdatedCorrectly()
    {
        // Arrange
        int numberOfEvents = 2;
        eventsSubSectionComponentController.eventsFromAPI = ExploreEventsTestHelpers.CreateTestEventsFromApi(numberOfEvents);

        // Act
        eventsSubSectionComponentController.OnRequestedEventsUpdated();
        
        // Assert
        eventsSubSectionComponentView.Received().SetFeaturedEvents(Arg.Any<List<EventCardComponentModel>>());
        eventsSubSectionComponentView.Received().SetTrendingEvents(Arg.Any<List<EventCardComponentModel>>());
        eventsSubSectionComponentView.Received().SetUpcomingEvents(Arg.Any<List<EventCardComponentModel>>());
        eventsSubSectionComponentView.Received().SetGoingEvents(Arg.Any<List<EventCardComponentModel>>());
        eventsSubSectionComponentView.Received().SetShowMoreUpcomingEventsButtonActive(eventsSubSectionComponentController.currentUpcomingEventsShowed < numberOfEvents);
    }

    [Test]
    public void LoadFeaturedEventsCorrectly()
    {
        // Arrange
        int numberOfEvents = 2;
        eventsSubSectionComponentController.eventsFromAPI = ExploreEventsTestHelpers.CreateTestEventsFromApi(numberOfEvents);

        // Act
        eventsSubSectionComponentController.LoadFeaturedEvents();

        // Assert
        eventsSubSectionComponentView.Received().SetFeaturedEvents(Arg.Any<List<EventCardComponentModel>>());
    }

    [Test]
    public void LoadTrendingEventsCorrectly()
    {
        // Arrange
        int numberOfEvents = 2;
        eventsSubSectionComponentController.eventsFromAPI = ExploreEventsTestHelpers.CreateTestEventsFromApi(numberOfEvents);

        // Act
        eventsSubSectionComponentController.LoadTrendingEvents();

        // Assert
        eventsSubSectionComponentView.Received().SetTrendingEvents(Arg.Any<List<EventCardComponentModel>>());
    }

    [Test]
    public void LoadUpcomingEventsCorrectly()
    {
        // Arrange
        int numberOfEvents = 2;
        eventsSubSectionComponentController.eventsFromAPI = ExploreEventsTestHelpers.CreateTestEventsFromApi(numberOfEvents);

        // Act
        eventsSubSectionComponentController.LoadUpcomingEvents();

        // Assert
        eventsSubSectionComponentView.Received().SetUpcomingEvents(Arg.Any<List<EventCardComponentModel>>());
        eventsSubSectionComponentView.Received().SetShowMoreUpcomingEventsButtonActive(eventsSubSectionComponentController.currentUpcomingEventsShowed < numberOfEvents);
    }

    [Test]
    [TestCase(2)]
    [TestCase(10)]
    public void LoaShowMoreUpcomingEventsCorrectly(int numberOfPlaces)
    {
        // Act
        eventsSubSectionComponentController.ShowMoreUpcomingEvents();

        // Assert
        eventsSubSectionComponentView.Received().SetShowMoreUpcomingEventsButtonActive(Arg.Any<bool>());
    }

    [Test]
    public void LoadGoingEventsCorrectly()
    {
        // Arrange
        int numberOfEvents = 2;
        eventsSubSectionComponentController.eventsFromAPI = ExploreEventsTestHelpers.CreateTestEventsFromApi(numberOfEvents);

        // Act
        eventsSubSectionComponentController.LoadGoingEvents();

        // Assert
        eventsSubSectionComponentView.Received().SetGoingEvents(Arg.Any<List<EventCardComponentModel>>());
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
        bool exploreClosed = false;
        eventsSubSectionComponentController.OnCloseExploreV2 += () => exploreClosed = true;
        EventFromAPIModel testEventFromAPI = ExploreEventsTestHelpers.CreateTestEvent("1");

        // Act
        eventsSubSectionComponentController.JumpInToEvent(testEventFromAPI);

        // Assert
        eventsSubSectionComponentView.Received().HideEventModal();
        Assert.IsTrue(exploreClosed);
        exploreV2Analytics.Received().SendEventTeleport(testEventFromAPI.id, testEventFromAPI.name, new Vector2Int(testEventFromAPI.coordinates[0], testEventFromAPI.coordinates[1]));
    }
}