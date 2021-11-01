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

    [SetUp]
    public void SetUp()
    {
        eventsSubSectionComponentView = Substitute.For<IEventsSubSectionComponentView>();
        eventsAPIController = Substitute.For<IEventsAPIController>();
        eventsSubSectionComponentController = new EventsSubSectionComponentController(eventsSubSectionComponentView, eventsAPIController);
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
        eventsSubSectionComponentView.Received().SetFeaturedEventsAsLoading(true);
        eventsSubSectionComponentView.Received().SetTrendingEventsAsLoading(true);
        eventsSubSectionComponentView.Received().SetUpcomingEventsAsLoading(true);
        eventsSubSectionComponentView.Received().SetShowMoreUpcomingEventsButtonActive(false);
        eventsSubSectionComponentView.Received().SetGoingEventsAsLoading(true);
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

        // Act
        eventsSubSectionComponentController.RequestAllEvents();

        // Assert
        Assert.AreEqual(eventsSubSectionComponentView.currentUpcomingEventsPerRow * EventsSubSectionComponentController.INITIAL_NUMBER_OF_UPCOMING_ROWS, eventsSubSectionComponentController.currentUpcomingEventsShowed);
        eventsSubSectionComponentView.Received().RestartScrollViewPosition();
        eventsSubSectionComponentView.Received().SetFeaturedEventsAsLoading(true);
        eventsSubSectionComponentView.Received().SetTrendingEventsAsLoading(true);
        eventsSubSectionComponentView.Received().SetUpcomingEventsAsLoading(true);
        eventsSubSectionComponentView.Received().SetShowMoreUpcomingEventsButtonActive(false);
        eventsSubSectionComponentView.Received().SetGoingEventsAsLoading(true);
        eventsAPIController.Received().GetAllEventsSigned(Arg.Any<Action<List<EventFromAPIModel>>>(), Arg.Any<Action<string>>());
        Assert.IsFalse(eventsSubSectionComponentController.reloadEvents);
    }

    [Test]
    public void RequestAllEventsFromAPICorrectly()
    {
        // Act
        eventsSubSectionComponentController.RequestAllEventsFromAPI();

        // Assert
        eventsAPIController.Received().GetAllEventsSigned(Arg.Any<Action<List<EventFromAPIModel>>>(), Arg.Any<Action<string>>());
    }

    [Test]
    public void RaiseOnRequestedEventsUpdatedCorrectly()
    {
        // Arrange
        int numberOfEvents = 2;
        eventsSubSectionComponentController.eventsFromAPI = CreateTestEventsFromApi(numberOfEvents);

        // Act
        eventsSubSectionComponentController.OnRequestedEventsUpdated();

        // Assert
        eventsSubSectionComponentView.Received().SetFeaturedEvents(Arg.Any<List<EventCardComponentModel>>());
        eventsSubSectionComponentView.Received().SetFeaturedEventsAsLoading(false);
        eventsSubSectionComponentView.Received().SetTrendingEvents(Arg.Any<List<EventCardComponentModel>>());
        eventsSubSectionComponentView.Received().SetTrendingEventsAsLoading(false);
        eventsSubSectionComponentView.Received().SetUpcomingEvents(Arg.Any<List<EventCardComponentModel>>());
        eventsSubSectionComponentView.Received().SetShowMoreUpcomingEventsButtonActive(eventsSubSectionComponentController.currentUpcomingEventsShowed < numberOfEvents);
        eventsSubSectionComponentView.Received().SetUpcomingEventsAsLoading(false);
        eventsSubSectionComponentView.Received().SetGoingEvents(Arg.Any<List<EventCardComponentModel>>());
        eventsSubSectionComponentView.Received().SetGoingEventsAsLoading(false);
    }

    [Test]
    public void LoadFeaturedEventsCorrectly()
    {
        // Arrange
        int numberOfEvents = 2;
        eventsSubSectionComponentController.eventsFromAPI = CreateTestEventsFromApi(numberOfEvents);

        // Act
        eventsSubSectionComponentController.LoadFeaturedEvents();

        // Assert
        eventsSubSectionComponentView.Received().SetFeaturedEvents(Arg.Any<List<EventCardComponentModel>>());
        eventsSubSectionComponentView.Received().SetFeaturedEventsAsLoading(false);
    }

    [Test]
    public void LoadTrendingEventsCorrectly()
    {
        // Arrange
        int numberOfEvents = 2;
        eventsSubSectionComponentController.eventsFromAPI = CreateTestEventsFromApi(numberOfEvents);

        // Act
        eventsSubSectionComponentController.LoadTrendingEvents();

        // Assert
        eventsSubSectionComponentView.Received().SetTrendingEvents(Arg.Any<List<EventCardComponentModel>>());
        eventsSubSectionComponentView.Received().SetTrendingEventsAsLoading(false);
    }

    [Test]
    public void LoadUpcomingEventsCorrectly()
    {
        // Arrange
        int numberOfEvents = 2;
        eventsSubSectionComponentController.eventsFromAPI = CreateTestEventsFromApi(numberOfEvents);

        // Act
        eventsSubSectionComponentController.LoadUpcomingEvents();

        // Assert
        eventsSubSectionComponentView.Received().SetUpcomingEvents(Arg.Any<List<EventCardComponentModel>>());
        eventsSubSectionComponentView.Received().SetShowMoreUpcomingEventsButtonActive(eventsSubSectionComponentController.currentUpcomingEventsShowed < numberOfEvents);
        eventsSubSectionComponentView.Received().SetUpcomingEventsAsLoading(false);
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
        eventsSubSectionComponentController.eventsFromAPI = CreateTestEventsFromApi(numberOfEvents);

        // Act
        eventsSubSectionComponentController.LoadGoingEvents();

        // Assert
        eventsSubSectionComponentView.Received().SetGoingEvents(Arg.Any<List<EventCardComponentModel>>());
        eventsSubSectionComponentView.Received().SetGoingEventsAsLoading(false);
    }

    [Test]
    public void CreateEventCardModelFromAPIEventCorrectly()
    {
        // Arrange
        EventFromAPIModel testEventFromAPI = CreateTestEvent("1");

        // Act
        EventCardComponentModel eventCardModel = eventsSubSectionComponentController.CreateEventCardModelFromAPIEvent(testEventFromAPI);

        // Assert
        Assert.AreEqual(testEventFromAPI.id, eventCardModel.eventId);
        Assert.AreEqual(testEventFromAPI.image, eventCardModel.eventPictureUri);
        Assert.AreEqual(testEventFromAPI.live, eventCardModel.isLive);
        Assert.AreEqual(EventsSubSectionComponentController.LIVE_TAG_TEXT, eventCardModel.liveTagText);
        Assert.AreEqual(eventsSubSectionComponentController.FormatEventDate(testEventFromAPI), eventCardModel.eventDateText);
        Assert.AreEqual(testEventFromAPI.name, eventCardModel.eventName);
        Assert.AreEqual(testEventFromAPI.description, eventCardModel.eventDescription);
        Assert.AreEqual(eventsSubSectionComponentController.FormatEventStartDate(testEventFromAPI), eventCardModel.eventStartedIn);
        Assert.AreEqual(eventsSubSectionComponentController.FormatEventStartDateFromTo(testEventFromAPI), eventCardModel.eventStartsInFromTo);
        Assert.AreEqual(eventsSubSectionComponentController.FormatEventOrganized(testEventFromAPI), eventCardModel.eventOrganizer);
        Assert.AreEqual(eventsSubSectionComponentController.FormatEventPlace(testEventFromAPI), eventCardModel.eventPlace);
        Assert.AreEqual(testEventFromAPI.total_attendees, eventCardModel.subscribedUsers);
        Assert.AreEqual(false, eventCardModel.isSubscribed);
        Assert.AreEqual(new Vector2Int(testEventFromAPI.coordinates[0], testEventFromAPI.coordinates[1]), eventCardModel.coords);
        Assert.AreEqual(testEventFromAPI, eventCardModel.eventFromAPIInfo);
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
    }

    [Test]
    public void JumpInToEventCorrectly()
    {
        // Arrange
        bool exploreClosed = false;
        eventsSubSectionComponentController.OnCloseExploreV2 += () => exploreClosed = true;
        EventFromAPIModel testEventFromAPI = CreateTestEvent("1");

        // Act
        eventsSubSectionComponentController.JumpInToEvent(testEventFromAPI);

        // Assert
        eventsSubSectionComponentView.Received().HideEventModal();
        Assert.IsTrue(exploreClosed);
    }

    [Test]
    public void SubscribeToEventCorrectly()
    {
        // Arrange
        string testEventId = "1";

        // Act
        eventsSubSectionComponentController.SubscribeToEvent(testEventId);

        // Assert
        eventsAPIController.Received().RegisterAttendEvent(testEventId, true, Arg.Any<Action>(), Arg.Any<Action<string>>());
    }

    [Test]
    public void UnsubscribeToEventCorrectly()
    {
        // Arrange
        string testEventId = "1";

        // Act
        eventsSubSectionComponentController.UnsubscribeToEvent(testEventId);

        // Assert
        eventsAPIController.Received().RegisterAttendEvent(testEventId, false, Arg.Any<Action>(), Arg.Any<Action<string>>());
    }

    private List<EventFromAPIModel> CreateTestEventsFromApi(int numberOfEvents)
    {
        List<EventFromAPIModel> testEvents = new List<EventFromAPIModel>();

        for (int i = 0; i < numberOfEvents; i++)
        {
            testEvents.Add(CreateTestEvent((i + 1).ToString()));
        }

        return testEvents;
    }

    private EventFromAPIModel CreateTestEvent(string id)
    {
        return new EventFromAPIModel
        {
            id = id,
            attending = false,
            coordinates = new int[] { 10, 10 },
            description = "Test Description",
            finish_at = "2021-11-30T11:11:00.000Z",
            highlighted = false,
            image = "Test Uri",
            live = true,
            name = "Test Name",
            next_start_at = "2021-09-30T11:11:00.000Z",
            realm = null,
            scene_name = "Test Scene Name",
            total_attendees = 100,
            trending = false,
            user_name = "Test User Name"
        };
    }
}