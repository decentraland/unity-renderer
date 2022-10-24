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
    public void SetFeaturedEventsAsLoadingCorrectly()
    {
        // Arrange
        eventsSubSectionComponent.featuredEvents.gameObject.SetActive(true);
        eventsSubSectionComponent.featuredEventsLoading.SetActive(false);

        // Act
        eventsSubSectionComponent.SetFeaturedEventsGroupAsLoading();

        // Assert
        Assert.AreEqual(false, eventsSubSectionComponent.featuredEvents.gameObject.activeSelf);
        Assert.AreEqual(true, eventsSubSectionComponent.featuredEventsLoading.activeSelf);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void SetTrendingEventsAsLoadingCorrectly(bool isVisible) =>
        SetEventsAsLoadingCorrectly(isVisible, eventsSubSectionComponent.trendingEvents.gameObject, eventsSubSectionComponent.trendingEventsLoading, eventsSubSectionComponent.trendingEventsNoDataText.gameObject);

    [TestCase(true)]
    [TestCase(false)]
    public void SetUpcomingEventsAsLoadingCorrectly(bool isVisible) =>
        SetEventsAsLoadingCorrectly(isVisible, eventsSubSectionComponent.upcomingEvents.gameObject, eventsSubSectionComponent.upcomingEventsLoading, eventsSubSectionComponent.upcomingEventsNoDataText.gameObject);

    [TestCase(true)]
    [TestCase(false)]
    public void SetGoingEventsAsLoadingCorrectly(bool isVisible) =>
        SetEventsAsLoadingCorrectly(isVisible, eventsSubSectionComponent.goingEvents.gameObject, eventsSubSectionComponent.goingEventsLoading, eventsSubSectionComponent.goingEventsNoDataText.gameObject);

    private void SetEventsAsLoadingCorrectly(bool isVisible, GameObject events, GameObject loadingBar, GameObject NoDataText = null)
    {
        // Arrange
        Canvas canvas = events.GetComponent<Canvas>();

        canvas.enabled = isVisible;
        loadingBar.SetActive(!isVisible);

        // Act
        eventsSubSectionComponent.SetEventsGroupAsLoading(isVisible, canvas, loadingBar);

        // Assert
        Assert.AreEqual(!isVisible, canvas.enabled);
        Assert.AreEqual(isVisible, loadingBar.activeSelf);

        if (isVisible && NoDataText != null)
            Assert.IsFalse(NoDataText.activeSelf);
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
    public void SetUpcomingEventsCorrectly()
    {
        // Arrange
        List<EventCardComponentModel> testEvents = ExploreEventsTestHelpers.CreateTestEvents(testSprite);

        // Act
        eventsSubSectionComponent.SetUpcomingEvents(testEvents);

        // Assert
        Assert.AreEqual(2, eventsSubSectionComponent.upcomingEvents.instantiatedItems.Count, "The number of set events does not match.");
        Assert.IsTrue(eventsSubSectionComponent.upcomingEvents.instantiatedItems.Any(x => (x as EventCardComponentView)?.model == testEvents[0]), "The event 1 is not contained in the places grid");
        Assert.IsTrue(eventsSubSectionComponent.upcomingEvents.instantiatedItems.Any(x => (x as EventCardComponentView)?.model == testEvents[1]), "The event 2 is not contained in the places grid");
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
}