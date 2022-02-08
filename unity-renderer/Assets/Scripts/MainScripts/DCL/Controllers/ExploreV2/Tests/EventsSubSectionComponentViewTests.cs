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
        eventsSubSectionComponent.ConfigurePools();
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
        GameObject.Destroy(eventsSubSectionComponent.eventModal.gameObject);
        GameObject.Destroy(eventsSubSectionComponent.gameObject);
        GameObject.Destroy(testTexture);
        GameObject.Destroy(testSprite);
    }

    [Test]
    public void SetFeaturedEventsCorrectly()
    {
        // Arrange
        List<EventCardComponentModel> testEvents = ExploreEventsTestHelpers.CreateTestEvents(testSprite);

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
    [TestCase(true)]
    [TestCase(false)]
    public void SetFeaturedEventsActiveCorrectly(bool isActive)
    {
        // Arrange
        eventsSubSectionComponent.featuredEvents.gameObject.SetActive(!isActive);

        // Act
        eventsSubSectionComponent.SetFeaturedEventsActive(isActive);

        // Assert
        Assert.AreEqual(isActive, eventsSubSectionComponent.featuredEvents.gameObject.activeSelf);
    }

    [Test]
    public void SetTrendingEventsCorrectly()
    {
        // Arrange
        List<EventCardComponentModel> testEvents = ExploreEventsTestHelpers.CreateTestEvents(testSprite);

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
        List<EventCardComponentModel> testEvents = ExploreEventsTestHelpers.CreateTestEvents(testSprite);

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
        List<EventCardComponentModel> testEvents = ExploreEventsTestHelpers.CreateTestEvents(testSprite);

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
        List<EventCardComponentModel> testEvents = ExploreEventsTestHelpers.CreateTestEvents(testSprite);

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
    public void ShowEventModalCorrectly()
    {
        // Arrange
        EventCardComponentModel testEventInfo = ExploreEventsTestHelpers.CreateTestEvent("1", testSprite);

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
}