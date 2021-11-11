using NUnit.Framework;
using UnityEngine;

public class EventCardHelpersTests
{
    private EventsSubSectionComponentView eventsSubSectionComponent;
    private EventCardComponentView testEventCard;
    private Texture2D testTexture;
    private Sprite testSprite;

    [SetUp]
    public void SetUp()
    {
        eventsSubSectionComponent = Object.Instantiate(Resources.Load<GameObject>("Sections/PlacesAndEventsSection/EventsSubSection/EventsSubSection")).GetComponent<EventsSubSectionComponentView>();
        eventsSubSectionComponent.Start();

        testEventCard = Object.Instantiate(Resources.Load<GameObject>("Sections/PlacesAndEventsSection/EventsSubSection/EventCard_Modal")).GetComponent<EventCardComponentView>();

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
        testEventCard.Dispose();
        GameObject.Destroy(eventsSubSectionComponent.eventModal.gameObject);
        GameObject.Destroy(eventsSubSectionComponent.gameObject);
        GameObject.Destroy(testEventCard.gameObject);
        GameObject.Destroy(testTexture);
        GameObject.Destroy(testSprite);
    }

    [Test]
    public void ConfigureEventCardModalCorrectly()
    {
        // Arrange
        GameObject.Destroy(eventsSubSectionComponent.eventModal);
        eventsSubSectionComponent.eventModal = null;

        // Act
        eventsSubSectionComponent.eventModal = EventCardHelpers.ConfigureEventCardModal(eventsSubSectionComponent.eventCardModalPrefab);

        // Assert
        Assert.IsNotNull(eventsSubSectionComponent.eventModal);
        Assert.AreEqual(EventCardHelpers.EVENT_CARD_MODAL_ID, eventsSubSectionComponent.eventModal.gameObject.name);
    }

    [Test]
    public void ConfigureEventCardsPoolCorrectly()
    {
        // Arrange
        eventsSubSectionComponent.featuredEventCardsPool = null;

        // Act
        EventCardHelpers.ConfigureEventCardsPool(
            out eventsSubSectionComponent.featuredEventCardsPool,
            EventsSubSectionComponentView.FEATURED_EVENT_CARDS_POOL_NAME,
            eventsSubSectionComponent.eventCardLongPrefab,
            10);

        // Assert
        Assert.IsNotNull(eventsSubSectionComponent.featuredEventCardsPool);
        Assert.AreEqual(EventsSubSectionComponentView.FEATURED_EVENT_CARDS_POOL_NAME, eventsSubSectionComponent.featuredEventCardsPool.id);
    }

    [Test]
    public void ConfigureEventCardCorrectly()
    {
        // Arrange
        EventCardComponentModel testEventInfo = CreateTestEvent("1");

        // Act
        EventCardHelpers.ConfigureEventCard(testEventCard, testEventInfo, null, null, null, null);

        // Assert
        Assert.AreEqual(testEventInfo, testEventCard.model, "The event card model does not match.");
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