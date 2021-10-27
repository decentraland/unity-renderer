using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventsSubSectionComponentViewTests
{
    private EventsSubSectionComponentView eventsSubSectionComponent;
    private Texture2D testTexture;
    private Sprite testSprite;

    [SetUp]
    public void SetUp()
    {
        eventsSubSectionComponent = Object.Instantiate(Resources.Load<GameObject>("Sections/PlacesAndEventsSection/EventsSubSection/EventsSubSection")).GetComponent<EventsSubSectionComponentView>();
        eventsSubSectionComponent.Start();
        testTexture = new Texture2D(20, 20);
        testSprite = Sprite.Create(testTexture, new Rect(), Vector2.zero);
    }

    [TearDown]
    public void TearDown()
    {
        eventsSubSectionComponent.featuredEvents.ExtractItems();
        eventsSubSectionComponent.featuredEventCardsPool.ReleaseAll();
        eventsSubSectionComponent.trendingEvents.ExtractItems();
        eventsSubSectionComponent.trendingEventCardsPool.ReleaseAll();
        eventsSubSectionComponent.upcomingEvents.ExtractItems();
        eventsSubSectionComponent.upcomingEventCardsPool.ReleaseAll();
        eventsSubSectionComponent.goingEvents.ExtractItems();
        eventsSubSectionComponent.goingEventCardsPool.ReleaseAll();
        eventsSubSectionComponent.Dispose();
        GameObject.Destroy(eventsSubSectionComponent.gameObject);
        GameObject.Destroy(testTexture);
        GameObject.Destroy(testSprite);
    }

    [Test]
    public void SetFeaturedEventsCorrectly()
    {
        // Arrange
        List<EventCardComponentModel> testEvents = CreateTestEvents();

        // Act
        eventsSubSectionComponent.SetFeaturedEvents(testEvents);

        // Assert
        Assert.AreEqual(2, eventsSubSectionComponent.featuredEvents.instantiatedItems.Count, "The number of set events does not match.");
        Assert.IsTrue(eventsSubSectionComponent.featuredEvents.instantiatedItems.Any(x => (x as EventCardComponentView).model == testEvents[0]), "The event 1 is not contained in the places carousel");
        Assert.IsTrue(eventsSubSectionComponent.featuredEvents.instantiatedItems.Any(x => (x as EventCardComponentView).model == testEvents[1]), "The event 2 is not contained in the places carousel");
        Assert.IsTrue(eventsSubSectionComponent.featuredEvents.gameObject.activeSelf, "The featuredEvents section should be visible.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetFeaturedEventsAsLoadingCorrectly(bool isVisible)
    {
        // Arrange
        eventsSubSectionComponent.featuredEvents.gameObject.SetActive(isVisible);
        eventsSubSectionComponent.featuredEventsLoading.SetActive(!isVisible);

        // Act
        eventsSubSectionComponent.SetFeaturedEventsAsLoading(isVisible);

        // Assert
        Assert.AreEqual(!isVisible, eventsSubSectionComponent.featuredEvents.gameObject.activeSelf);
        Assert.AreEqual(isVisible, eventsSubSectionComponent.featuredEventsLoading.activeSelf);
    }

    [Test]
    public void SetTrendingEventsCorrectly()
    {
        // Arrange
        List<EventCardComponentModel> testEvents = CreateTestEvents();

        // Act
        eventsSubSectionComponent.SetTrendingEvents(testEvents);

        // Assert
        Assert.AreEqual(2, eventsSubSectionComponent.trendingEvents.instantiatedItems.Count, "The number of set events does not match.");
        Assert.IsTrue(eventsSubSectionComponent.trendingEvents.instantiatedItems.Any(x => (x as EventCardComponentView).model == testEvents[0]), "The event 1 is not contained in the places grid");
        Assert.IsTrue(eventsSubSectionComponent.trendingEvents.instantiatedItems.Any(x => (x as EventCardComponentView).model == testEvents[1]), "The event 2 is not contained in the places grid");
        Assert.IsFalse(eventsSubSectionComponent.trendingEventsNoDataText.gameObject.activeSelf, "The trendingEventsNoDataText should be visible.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetTrendingEventsAsLoadingCorrectly(bool isVisible)
    {
        // Arrange
        eventsSubSectionComponent.trendingEvents.gameObject.SetActive(isVisible);
        eventsSubSectionComponent.trendingEventsLoading.SetActive(!isVisible);

        // Act
        eventsSubSectionComponent.SetTrendingEventsAsLoading(isVisible);

        // Assert
        Assert.AreEqual(!isVisible, eventsSubSectionComponent.trendingEvents.gameObject.activeSelf);
        Assert.AreEqual(isVisible, eventsSubSectionComponent.trendingEventsLoading.activeSelf);

        if (isVisible)
            Assert.IsFalse(eventsSubSectionComponent.trendingEventsNoDataText.gameObject.activeSelf);
    }

    [Test]
    public void SetUpcomingEventsCorrectly()
    {
        // Arrange
        List<EventCardComponentModel> testEvents = CreateTestEvents();

        // Act
        eventsSubSectionComponent.SetUpcomingEvents(testEvents);

        // Assert
        Assert.AreEqual(2, eventsSubSectionComponent.upcomingEvents.instantiatedItems.Count, "The number of set events does not match.");
        Assert.IsTrue(eventsSubSectionComponent.upcomingEvents.instantiatedItems.Any(x => (x as EventCardComponentView).model == testEvents[0]), "The event 1 is not contained in the places grid");
        Assert.IsTrue(eventsSubSectionComponent.upcomingEvents.instantiatedItems.Any(x => (x as EventCardComponentView).model == testEvents[1]), "The event 2 is not contained in the places grid");
        Assert.IsFalse(eventsSubSectionComponent.upcomingEventsNoDataText.gameObject.activeSelf, "The upcomingEventsNoDataText should be visible.");
    }

    [Test]
    public void AddUpcomingEventsCorrectly()
    {
        // Arrange
        eventsSubSectionComponent.upcomingEvents.RemoveItems();
        List<EventCardComponentModel> testEvents = CreateTestEvents();

        // Act
        eventsSubSectionComponent.AddUpcomingEvents(testEvents);

        // Assert
        Assert.AreEqual(2, eventsSubSectionComponent.upcomingEvents.instantiatedItems.Count, "The number of set events does not match.");
        Assert.IsTrue(eventsSubSectionComponent.upcomingEvents.instantiatedItems.Any(x => (x as EventCardComponentView).model == testEvents[0]), "The event 1 is not contained in the places grid");
        Assert.IsTrue(eventsSubSectionComponent.upcomingEvents.instantiatedItems.Any(x => (x as EventCardComponentView).model == testEvents[1]), "The event 2 is not contained in the places grid");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetUpcomingEventsAsLoadingCorrectly(bool isVisible)
    {
        // Arrange
        eventsSubSectionComponent.upcomingEvents.gameObject.SetActive(isVisible);
        eventsSubSectionComponent.upcomingEventsLoading.SetActive(!isVisible);

        // Act
        eventsSubSectionComponent.SetUpcomingEventsAsLoading(isVisible);

        // Assert
        Assert.AreEqual(!isVisible, eventsSubSectionComponent.upcomingEvents.gameObject.activeSelf);
        Assert.AreEqual(isVisible, eventsSubSectionComponent.upcomingEventsLoading.activeSelf);

        if (isVisible)
            Assert.IsFalse(eventsSubSectionComponent.upcomingEventsNoDataText.gameObject.activeSelf);
    }

    [Test]
    public void SetGoingEventsCorrectly()
    {
        // Arrange
        List<EventCardComponentModel> testEvents = CreateTestEvents();

        // Act
        eventsSubSectionComponent.SetGoingEvents(testEvents);

        // Assert
        Assert.AreEqual(2, eventsSubSectionComponent.goingEvents.instantiatedItems.Count, "The number of set events does not match.");
        Assert.IsTrue(eventsSubSectionComponent.goingEvents.instantiatedItems.Any(x => (x as EventCardComponentView).model == testEvents[0]), "The event 1 is not contained in the places grid");
        Assert.IsTrue(eventsSubSectionComponent.goingEvents.instantiatedItems.Any(x => (x as EventCardComponentView).model == testEvents[1]), "The event 2 is not contained in the places grid");
        Assert.IsFalse(eventsSubSectionComponent.goingEventsNoDataText.gameObject.activeSelf, "The goingEventsNoDataText should be visible.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetGoingEventsAsLoadingCorrectly(bool isVisible)
    {
        // Arrange
        eventsSubSectionComponent.goingEvents.gameObject.SetActive(isVisible);
        eventsSubSectionComponent.goingEventsLoading.SetActive(!isVisible);

        // Act
        eventsSubSectionComponent.SetGoingEventsAsLoading(isVisible);

        // Assert
        Assert.AreEqual(!isVisible, eventsSubSectionComponent.goingEvents.gameObject.activeSelf);
        Assert.AreEqual(isVisible, eventsSubSectionComponent.goingEventsLoading.activeSelf);

        if (isVisible)
            Assert.IsFalse(eventsSubSectionComponent.goingEventsNoDataText.gameObject.activeSelf);
    }

    [Test]
    public void ShowPlaceModalCorrectly()
    {
        // Arrange
        EventCardComponentModel testEventInfo = CreateTestEvent("1");

        // Act
        eventsSubSectionComponent.ShowEventModal(testEventInfo);

        // Assert
        Assert.AreEqual(testEventInfo, eventsSubSectionComponent.eventModal.model, "The event modal model does not match.");

        eventsSubSectionComponent.HideEventModal();
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetShowMorePlacesButtonActiveCorrectly(bool isVisible)
    {
        // Arrange
        eventsSubSectionComponent.showMoreUpcomingEventsButtonContainer.gameObject.SetActive(!isVisible);

        // Act
        eventsSubSectionComponent.SetShowMoreUpcomingEventsButtonActive(isVisible);

        // Assert
        Assert.AreEqual(isVisible, eventsSubSectionComponent.showMoreUpcomingEventsButtonContainer.gameObject.activeSelf);
    }

    [Test]
    public void ConfigureEventCardModalCorrectly()
    {
        // Arrange
        GameObject.Destroy(eventsSubSectionComponent.eventModal);
        eventsSubSectionComponent.eventModal = null;

        // Act
        eventsSubSectionComponent.ConfigureEventCardModal();

        // Assert
        Assert.IsNotNull(eventsSubSectionComponent.eventModal);
    }

    [Test]
    public void ConfigureFeaturedEventCardsPoolCorrectly()
    {
        // Arrange
        eventsSubSectionComponent.featuredEventCardsPool = null;

        // Act
        eventsSubSectionComponent.ConfigureEventCardsPool(
            out eventsSubSectionComponent.featuredEventCardsPool,
            EventsSubSectionComponentView.FEATURED_EVENT_CARDS_POOL_NAME,
            eventsSubSectionComponent.eventCardLongPrefab,
            10);

        // Assert
        Assert.IsNotNull(eventsSubSectionComponent.featuredEventCardsPool);
        Assert.AreEqual(EventsSubSectionComponentView.FEATURED_EVENT_CARDS_POOL_NAME, eventsSubSectionComponent.featuredEventCardsPool.id);
    }

    [Test]
    public void ConfigureTrendingEventCardsPoolCorrectly()
    {
        // Arrange
        eventsSubSectionComponent.trendingEventCardsPool = null;

        // Act
        eventsSubSectionComponent.ConfigureEventCardsPool(
            out eventsSubSectionComponent.trendingEventCardsPool,
            EventsSubSectionComponentView.TRENDING_EVENT_CARDS_POOL_NAME,
            eventsSubSectionComponent.eventCardLongPrefab,
            10);

        // Assert
        Assert.IsNotNull(eventsSubSectionComponent.trendingEventCardsPool);
        Assert.AreEqual(EventsSubSectionComponentView.TRENDING_EVENT_CARDS_POOL_NAME, eventsSubSectionComponent.trendingEventCardsPool.id);
    }

    [Test]
    public void ConfigureUpcomingEventCardsPoolCorrectly()
    {
        // Arrange
        eventsSubSectionComponent.upcomingEventCardsPool = null;

        // Act
        eventsSubSectionComponent.ConfigureEventCardsPool(
            out eventsSubSectionComponent.upcomingEventCardsPool,
            EventsSubSectionComponentView.UPCOMING_EVENT_CARDS_POOL_NAME,
            eventsSubSectionComponent.eventCardLongPrefab,
            10);

        // Assert
        Assert.IsNotNull(eventsSubSectionComponent.upcomingEventCardsPool);
        Assert.AreEqual(EventsSubSectionComponentView.UPCOMING_EVENT_CARDS_POOL_NAME, eventsSubSectionComponent.upcomingEventCardsPool.id);
    }

    [Test]
    public void ConfigureGoingEventCardsPoolCorrectly()
    {
        // Arrange
        eventsSubSectionComponent.goingEventCardsPool = null;

        // Act
        eventsSubSectionComponent.ConfigureEventCardsPool(
            out eventsSubSectionComponent.goingEventCardsPool,
            EventsSubSectionComponentView.GOING_EVENT_CARDS_POOL_NAME,
            eventsSubSectionComponent.eventCardLongPrefab,
            10);

        // Assert
        Assert.IsNotNull(eventsSubSectionComponent.goingEventCardsPool);
        Assert.AreEqual(EventsSubSectionComponentView.GOING_EVENT_CARDS_POOL_NAME, eventsSubSectionComponent.goingEventCardsPool.id);
    }

    private List<EventCardComponentModel> CreateTestEvents()
    {
        List<EventCardComponentModel> testEvents = new List<EventCardComponentModel>();
        testEvents.Add(CreateTestEvent("1"));
        testEvents.Add(CreateTestEvent("2"));

        return testEvents;
    }

    private EventCardComponentModel CreateTestEvent(string id)
    {
        return new EventCardComponentModel
        {
            eventId = id,
            coords = new Vector2Int(19, 10),
            eventDateText = "Test Date",
            eventDescription = "Test Description",
            eventName = "Test Name",
            eventOrganizer = "Test Organizer",
            eventPictureSprite = testSprite,
            eventPlace = "Test Place",
            eventStartedIn = "Test Date",
            eventStartsInFromTo = "Test Start",
            isLive = true,
            isSubscribed = false,
            liveTagText = "Test Live Text",
            subscribedUsers = 100,
            eventFromAPIInfo = new EventFromAPIModel
            {
                id = id,
                attending = false,
                coordinates = new int[] { 10, 10 },
                description = "Test Description",
                finish_at = "Test Date",
                highlighted = false,
                image = "Test Uri",
                live = true,
                name = "Test Name",
                next_start_at = "Test Start",
                realm = "Test Realm",
                scene_name = "Test Scene Name",
                total_attendees = 100,
                trending = false,
                user_name = "Test User Name"
            }
        };
    }
}