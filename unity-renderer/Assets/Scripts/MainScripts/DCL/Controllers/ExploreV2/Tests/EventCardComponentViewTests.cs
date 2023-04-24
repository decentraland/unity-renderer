using System.Collections;
using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EventCardComponentViewTests
{
    private EventCardComponentView eventCardComponent;
    private EventCardComponentView eventCardModalComponent;
    private Texture2D testTexture;
    private Sprite testSprite;

    [SetUp]
    public void SetUp()
    {
        eventCardComponent = BaseComponentView.Create<EventCardComponentView>("Sections/PlacesAndEventsSection/EventsSubSection/EventCard");
        eventCardModalComponent = BaseComponentView.Create<EventCardComponentView>("Sections/PlacesAndEventsSection/EventsSubSection/EventCard_Modal");
        eventCardComponent.eventImage.imageObserver = Substitute.For<ILazyTextureObserver>();
        eventCardModalComponent.eventImage.imageObserver = Substitute.For<ILazyTextureObserver>();
        testTexture = new Texture2D(20, 20);
        testSprite = Sprite.Create(testTexture, new Rect(), Vector2.zero);
        eventCardComponent.Start();
        eventCardModalComponent.Start();
        eventCardComponent.OnFocus();
        eventCardComponent.OnLoseFocus();
    }

    [TearDown]
    public void TearDown()
    {
        eventCardComponent.Dispose();
        eventCardModalComponent.Dispose();
        GameObject.Destroy(eventCardComponent.gameObject);
        GameObject.Destroy(eventCardModalComponent.gameObject);
        GameObject.Destroy(testTexture);
        GameObject.Destroy(testSprite);
    }

    [Test]
    public void ConfigureEventCardCorrectly()
    {
        // Arrange
        EventCardComponentModel testModel = new EventCardComponentModel
        {
            eventId = "1",
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
                id = "1",
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

        // Act
        eventCardComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, eventCardComponent.model, "The model does not match after configuring the event card.");
    }

    [Test]
    public void SetEventCardPictureFromSpriteCorrectly()
    {
        // Act
        eventCardComponent.SetEventPicture(testSprite);

        // Assert
        Assert.AreEqual(testSprite, eventCardComponent.model.eventPictureSprite, "The event card picture sprite does not match in the model.");
        Assert.AreEqual(testSprite, eventCardComponent.eventImage.image.sprite, "The event card image does not match.");
    }

    [Test]
    public void SetEventCardPictureFromTextureCorrectly()
    {
        // Arrange
        eventCardComponent.model.eventPictureTexture = null;

        // Act
        eventCardComponent.SetEventPicture(testTexture);

        // Assert
        Assert.AreEqual(testTexture, eventCardComponent.model.eventPictureTexture, "The event card picture texture does not match in the model.");
        eventCardComponent.eventImage.imageObserver.Received().RefreshWithTexture(testTexture);
    }

    [Test]
    public void SetEventCardPictureFromUriCorrectly()
    {
        // Arrange
        string testUri = "testUri";
        eventCardComponent.model.eventPictureUri = null;

        // Act
        eventCardComponent.SetEventPicture(testUri);

        // Assert
        Assert.AreEqual(testUri, eventCardComponent.model.eventPictureUri, "The event card picture uri does not match in the model.");
        eventCardComponent.eventImage.imageObserver.Received().RefreshWithUri(testUri);
    }

    [Test]
    [TestCase(true, true, true)]
    [TestCase(true, false, true)]
    [TestCase(false, true, true)]
    [TestCase(false, false, true)]
    [TestCase(true, true, false)]
    [TestCase(true, false, false)]
    [TestCase(false, true, false)]
    [TestCase(false, false, false)]
    public void SetEventAsLiveCorrectly(bool isLive, bool isSubscribed, bool isEventCardModal)
    {
        // Arrange
        eventCardComponent.model.isLive = !isLive;
        eventCardComponent.model.isSubscribed = isSubscribed;
        eventCardComponent.isEventCardModal = isEventCardModal;

        // Act
        eventCardComponent.SetEventAsLive(isLive);

        // Assert
        Assert.AreEqual(isLive, eventCardComponent.model.isLive, "The event card isLive does not match in the model.");
        Assert.AreEqual(isLive, eventCardComponent.liveTag.gameObject.activeSelf);
        Assert.AreEqual(!isLive, eventCardComponent.eventDateText.gameObject.activeSelf);
        Assert.AreEqual(isEventCardModal || isLive, eventCardComponent.jumpinButton.gameObject.activeSelf);
        Assert.AreEqual(!isLive && !eventCardComponent.model.eventFromAPIInfo.attending, eventCardComponent.subscribeEventButton.gameObject.activeSelf);
        Assert.AreEqual(!isLive && eventCardComponent.model.eventFromAPIInfo.attending, eventCardComponent.unsubscribeEventButton.gameObject.activeSelf);
        Assert.AreEqual(isLive, eventCardComponent.eventStartedInTitleForLive.gameObject.activeSelf);
        Assert.AreEqual(!isLive, eventCardComponent.subscribedUsersTitleForNotLive.gameObject.activeSelf);
    }

    [Test]
    public void SetLiveTagTextCorrectly()
    {
        // Arrange
        string testTageText = "Test text";

        // Act
        eventCardComponent.SetLiveTagText(testTageText);

        // Assert
        Assert.AreEqual(testTageText, eventCardComponent.model.liveTagText, "The event card liveTagText does not match in the model.");
        Assert.AreEqual(testTageText, eventCardComponent.liveTag.text.text);
    }

    [Test]
    public void SetEventDateCorrectly()
    {
        // Arrange
        string testDate = "Test date";

        // Act
        eventCardComponent.SetEventDate(testDate);

        // Assert
        Assert.AreEqual(testDate, eventCardComponent.model.eventDateText, "The event card eventDateText does not match in the model.");
        Assert.AreEqual(testDate, eventCardComponent.eventDateText.text);
    }

    [Test]
    public void SetEventNameCorrectly()
    {
        // Arrange
        string testName = "Test name";

        // Act
        eventCardComponent.SetEventName(testName);

        // Assert
        Assert.AreEqual(testName, eventCardComponent.model.eventName, "The event card eventName does not match in the model.");
        Assert.AreEqual(testName, eventCardComponent.eventNameText.text);
    }

    [Test]
    public void SetEventDescriptionCorrectly()
    {
        // Arrange
        string testDesc = "Test decription";

        // Act
        eventCardModalComponent.SetEventDescription(testDesc);

        // Assert
        Assert.AreEqual(testDesc, eventCardModalComponent.model.eventDescription, "The event card eventDescription does not match in the model.");
        Assert.AreEqual(testDesc, eventCardModalComponent.eventDescText.text);
    }

    [Test]
    public void SetEventStartedInCorrectly()
    {
        // Arrange
        string testStartedIn = "Test StartedIn";

        // Act
        eventCardComponent.SetEventStartedIn(testStartedIn);

        // Assert
        Assert.AreEqual(testStartedIn, eventCardComponent.model.eventStartedIn, "The event card eventStartedIn does not match in the model.");
        Assert.AreEqual(testStartedIn, eventCardComponent.eventStartedInText.text);
    }

    [Test]
    public void SetEventStartsInFromToCorrectly()
    {
        // Arrange
        string testText = "Test text";

        // Act
        eventCardModalComponent.SetEventStartsInFromTo(testText);

        // Assert
        Assert.AreEqual(testText, eventCardModalComponent.model.eventStartsInFromTo, "The event card eventStartsInFromTo does not match in the model.");
        Assert.AreEqual(testText, eventCardModalComponent.eventStartsInFromToText.text);
    }

    [Test]
    public void SetEventOrganizerCorrectly()
    {
        // Arrange
        string testText = "Test text";

        // Act
        eventCardModalComponent.SetEventOrganizer(testText);

        // Assert
        Assert.AreEqual(testText, eventCardModalComponent.model.eventOrganizer, "The event card eventOrganizer does not match in the model.");
        Assert.AreEqual(testText, eventCardModalComponent.eventOrganizerText.text);
    }

    [Test]
    public void SetEventPlaceCorrectly()
    {
        // Arrange
        string testText = "Test text";

        // Act
        eventCardModalComponent.SetEventPlace(testText);

        // Assert
        Assert.AreEqual(testText, eventCardModalComponent.model.eventPlace, "The event card eventPlace does not match in the model.");
        Assert.AreEqual(testText, eventCardModalComponent.eventPlaceText.text);
    }

    [Test]
    [TestCase(true, 0)]
    [TestCase(true, 10)]
    [TestCase(false, 0)]
    [TestCase(false, 10)]
    public void SetSubscribersUsersCorrectly(bool isEventCardModal, int newNumberOfUsers)
    {
        // Arrange
        eventCardComponent.isEventCardModal = isEventCardModal;

        // Act
        eventCardComponent.SetSubscribersUsers(newNumberOfUsers);

        // Assert
        Assert.AreEqual(newNumberOfUsers, eventCardComponent.model.subscribedUsers, "The event card subscribedUsers does not match in the model.");
        if (!isEventCardModal)
        {
            Assert.AreEqual(newNumberOfUsers.ToString() + " going", eventCardComponent.subscribedUsersText.text);
        }
        else
        {
            if (newNumberOfUsers > 0)
                Assert.AreEqual(string.Format(EventCardComponentView.USERS_CONFIRMED_MESSAGE, newNumberOfUsers), eventCardComponent.subscribedUsersText.text);
            else
                Assert.AreEqual(EventCardComponentView.NOBODY_CONFIRMED_MESSAGE, eventCardComponent.subscribedUsersText.text);
        }
    }

    [Test]
    public void SetCoordsCorrectly()
    {
        // Arrange
        Vector2Int testCoords = new Vector2Int(20, 20);

        // Act
        eventCardModalComponent.SetCoords(testCoords);

        // Assert
        Assert.AreEqual(testCoords, eventCardModalComponent.model.coords, "The event card coords does not match in the model.");
        Assert.AreEqual($"{testCoords.x},{testCoords.y}", eventCardModalComponent.jumpinButton.model.text);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetLoadingIndicatorVisibleCorrectly(bool isVisible)
    {
        // Arrange
        eventCardComponent.imageContainer.SetActive(isVisible);
        eventCardComponent.eventInfoContainer.SetActive(isVisible);
        eventCardComponent.loadingSpinner.SetActive(!isVisible);

        // Act
        eventCardComponent.SetLoadingIndicatorVisible(isVisible);

        // Assert
        Assert.AreEqual(!isVisible, eventCardComponent.imageContainer.activeSelf, "The image container active property does not match.");
        Assert.AreEqual(!isVisible, eventCardComponent.eventInfoContainer.activeSelf, "The info container active property does not match.");
        Assert.AreEqual(isVisible, eventCardComponent.loadingSpinner.activeSelf, "The loading spinner active property does not match.");
    }

    [Test]
    public void CloseModalCorrectly()
    {
        // Arrange
        eventCardModalComponent.Show();

        // Act
        eventCardModalComponent.CloseModal();

        // Assert
        Assert.IsFalse(eventCardModalComponent.isVisible);
    }

    [Test]
    public void RaiseOnCloseActionTriggeredCorrectly()
    {
        // Arrange
        eventCardModalComponent.Show();

        // Act
        eventCardModalComponent.OnCloseActionTriggered(new DCLAction_Trigger());

        // Assert
        Assert.IsFalse(eventCardModalComponent.isVisible);
    }
}
