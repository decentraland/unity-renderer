using NUnit.Framework;
using UnityEngine;

public class ExploreEventsCommonTests
{
    private EventsSubSectionComponentView eventsSubSectionComponent;
    private EventCardComponentView testEventCard;
    private Texture2D testTexture;
    private Sprite testSprite;

    [SetUp]
    public void SetUp()
    {
        eventsSubSectionComponent = Object.Instantiate(Resources.Load<GameObject>("Sections/PlacesAndEventsSection/EventsSubSection/EventsSubSection")).GetComponent<EventsSubSectionComponentView>();
        eventsSubSectionComponent.ConfigurePools();
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
        eventsSubSectionComponent.eventModal = ExploreEventsUtils.ConfigureEventCardModal(eventsSubSectionComponent.eventCardModalPrefab);

        // Assert
        Assert.IsNotNull(eventsSubSectionComponent.eventModal);
        Assert.AreEqual(ExploreEventsUtils.EVENT_CARD_MODAL_ID, eventsSubSectionComponent.eventModal.gameObject.name);
    }

    [Test]
    public void ConfigureEventCardsPoolCorrectly()
    {
        // Arrange
        eventsSubSectionComponent.featuredEventCardsPool = null;

        // Act
        ExploreEventsUtils.ConfigureEventCardsPool(
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
        EventCardComponentModel testEventInfo = CreateTestEventModel("1");

        // Act
        ExploreEventsUtils.ConfigureEventCard(testEventCard, testEventInfo, null, null, null, null);

        // Assert
        Assert.AreEqual(testEventInfo, testEventCard.model, "The event card model does not match.");
    }

    [Test]
    public void CreateEventCardModelFromAPIEventCorrectly()
    {
        // Arrange
        EventFromAPIModel testEventFromAPI = CreateTestEventFromAPI("1");

        // Act
        EventCardComponentModel eventCardModel = ExploreEventsUtils.CreateEventCardModelFromAPIEvent(testEventFromAPI);

        // Assert
        Assert.AreEqual(testEventFromAPI.id, eventCardModel.eventId);
        Assert.AreEqual(testEventFromAPI.image, eventCardModel.eventPictureUri);
        Assert.AreEqual(testEventFromAPI.live, eventCardModel.isLive);
        Assert.AreEqual(ExploreEventsUtils.LIVE_TAG_TEXT, eventCardModel.liveTagText);
        Assert.AreEqual(ExploreEventsUtils.FormatEventDate(testEventFromAPI), eventCardModel.eventDateText);
        Assert.AreEqual(testEventFromAPI.name, eventCardModel.eventName);
        Assert.AreEqual(testEventFromAPI.description, eventCardModel.eventDescription);
        Assert.AreEqual(ExploreEventsUtils.FormatEventStartDate(testEventFromAPI), eventCardModel.eventStartedIn);
        Assert.AreEqual(ExploreEventsUtils.FormatEventStartDateFromTo(testEventFromAPI), eventCardModel.eventStartsInFromTo);
        Assert.AreEqual(ExploreEventsUtils.FormatEventOrganized(testEventFromAPI), eventCardModel.eventOrganizer);
        Assert.AreEqual(ExploreEventsUtils.FormatEventPlace(testEventFromAPI), eventCardModel.eventPlace);
        Assert.AreEqual(testEventFromAPI.total_attendees, eventCardModel.subscribedUsers);
        Assert.AreEqual(false, eventCardModel.isSubscribed);
        Assert.AreEqual(new Vector2Int(testEventFromAPI.coordinates[0], testEventFromAPI.coordinates[1]), eventCardModel.coords);
        Assert.AreEqual(testEventFromAPI, eventCardModel.eventFromAPIInfo);
    }

    private EventCardComponentModel CreateTestEventModel(string id)
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

    private EventFromAPIModel CreateTestEventFromAPI(string id)
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