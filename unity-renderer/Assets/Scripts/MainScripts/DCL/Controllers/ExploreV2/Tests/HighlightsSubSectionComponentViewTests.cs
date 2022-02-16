using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HighlightsSubSectionComponentViewTests
{
    private HighlightsSubSectionComponentView highlightsSubSectionComponent;
    private Texture2D testTexture;
    private Sprite testSprite;

    [SetUp]
    public void SetUp()
    {
        highlightsSubSectionComponent = Object.Instantiate(Resources.Load<GameObject>("Sections/PlacesAndEventsSection/HighlightsSubSection/HighlightsSubSection")).GetComponent<HighlightsSubSectionComponentView>();
        highlightsSubSectionComponent.ConfigurePools();
        highlightsSubSectionComponent.Start();
        testTexture = new Texture2D(20, 20);
        testSprite = Sprite.Create(testTexture, new Rect(), Vector2.zero);
    }

    [TearDown]
    public void TearDown()
    {
        highlightsSubSectionComponent.trendingPlacesAndEvents.ExtractItems();
        highlightsSubSectionComponent.trendingPlaceCardsPool.ReleaseAll();
        highlightsSubSectionComponent.featuredPlaces.ExtractItems();
        highlightsSubSectionComponent.featuredPlaceCardsPool.ReleaseAll();
        highlightsSubSectionComponent.liveEvents.ExtractItems();
        highlightsSubSectionComponent.liveEventCardsPool.ReleaseAll();
        highlightsSubSectionComponent.Dispose();
        GameObject.Destroy(highlightsSubSectionComponent.placeModal.gameObject);
        GameObject.Destroy(highlightsSubSectionComponent.eventModal.gameObject);
        GameObject.Destroy(highlightsSubSectionComponent.gameObject);
        GameObject.Destroy(testTexture);
        GameObject.Destroy(testSprite);
    }

    [Test]
    public void SetPromotedPlacesAndEventsCorrectly()
    {
        // Arrange
        List<PlaceCardComponentModel> testPlaces = ExplorePlacesTestHelpers.CreateTestPlaces(testSprite);
        List<EventCardComponentModel> testEvents = ExploreEventsTestHelpers.CreateTestEvents(testSprite);

        // Act
        highlightsSubSectionComponent.SetTrendingPlacesAndEvents(testPlaces, testEvents);

        // Assert
        Assert.AreEqual(4, highlightsSubSectionComponent.trendingPlacesAndEvents.instantiatedItems.Count, "The number of set places/events does not match.");
        Assert.IsTrue(highlightsSubSectionComponent.trendingPlacesAndEvents.instantiatedItems.Any(x => (x as PlaceCardComponentView) && (x as PlaceCardComponentView).model == testPlaces[0]), "The place 1 is not contained in the carousel");
        Assert.IsTrue(highlightsSubSectionComponent.trendingPlacesAndEvents.instantiatedItems.Any(x => (x as PlaceCardComponentView) && (x as PlaceCardComponentView).model == testPlaces[1]), "The place 2 is not contained in the carousel");
        Assert.IsTrue(highlightsSubSectionComponent.trendingPlacesAndEvents.instantiatedItems.Any(x => (x is EventCardComponentView) && (x as EventCardComponentView).model == testEvents[0]), "The event 1 is not contained in the carousel");
        Assert.IsTrue(highlightsSubSectionComponent.trendingPlacesAndEvents.instantiatedItems.Any(x => (x is EventCardComponentView) && (x as EventCardComponentView).model == testEvents[1]), "The event 2 is not contained in the carousel");
        Assert.IsTrue(highlightsSubSectionComponent.trendingPlacesAndEvents.gameObject.activeSelf, "The promotedPlaces section should be visible.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetPromotedPlacesAsLoadingCorrectly(bool isVisible)
    {
        // Arrange
        highlightsSubSectionComponent.trendingPlacesAndEvents.gameObject.SetActive(isVisible);
        highlightsSubSectionComponent.trendingPlacesAndEventsLoading.SetActive(!isVisible);

        // Act
        highlightsSubSectionComponent.SetTrendingPlacesAndEventsAsLoading(isVisible);

        // Assert
        Assert.AreEqual(!isVisible, highlightsSubSectionComponent.trendingPlacesAndEvents.gameObject.activeSelf);
        Assert.AreEqual(isVisible, highlightsSubSectionComponent.trendingPlacesAndEventsLoading.activeSelf);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetPromotedPlacesActiveCorrectly(bool isActive)
    {
        // Arrange
        highlightsSubSectionComponent.trendingPlacesAndEvents.gameObject.SetActive(!isActive);

        // Act
        highlightsSubSectionComponent.SetTrendingPlacesAndEventsActive(isActive);

        // Assert
        Assert.AreEqual(isActive, highlightsSubSectionComponent.trendingPlacesAndEvents.gameObject.activeSelf);
    }

    [Test]
    public void SetFeaturedPlacesCorrectly()
    {
        // Arrange
        List<PlaceCardComponentModel> testPlaces = ExplorePlacesTestHelpers.CreateTestPlaces(testSprite);

        // Act
        highlightsSubSectionComponent.SetFeaturedPlaces(testPlaces);

        // Assert
        Assert.AreEqual(2, highlightsSubSectionComponent.featuredPlaces.instantiatedItems.Count, "The number of set places does not match.");
        Assert.IsTrue(highlightsSubSectionComponent.featuredPlaces.instantiatedItems.Any(x => (x as PlaceCardComponentView).model == testPlaces[0]), "The place 1 is not contained in the places carousel");
        Assert.IsTrue(highlightsSubSectionComponent.featuredPlaces.instantiatedItems.Any(x => (x as PlaceCardComponentView).model == testPlaces[1]), "The place 2 is not contained in the places carousel");
        Assert.IsFalse(highlightsSubSectionComponent.featuredPlacesNoDataText.gameObject.activeSelf, "The featuredPlacesNoDataText should be visible.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetFeaturedPlacesAsLoadingCorrectly(bool isVisible)
    {
        // Arrange
        highlightsSubSectionComponent.featuredPlaces.gameObject.SetActive(isVisible);
        highlightsSubSectionComponent.featuredPlacesLoading.SetActive(!isVisible);
        highlightsSubSectionComponent.featuredPlacesNoDataText.gameObject.SetActive(true);

        // Act
        highlightsSubSectionComponent.SetFeaturedPlacesAsLoading(isVisible);

        // Assert
        Assert.AreEqual(!isVisible, highlightsSubSectionComponent.featuredPlaces.gameObject.activeSelf);
        Assert.AreEqual(isVisible, highlightsSubSectionComponent.featuredPlacesLoading.activeSelf);

        if (isVisible)
            Assert.IsFalse(highlightsSubSectionComponent.featuredPlacesNoDataText.gameObject.activeSelf);
    }

    [Test]
    public void SetLiveEventsCorrectly()
    {
        // Arrange
        List<EventCardComponentModel> testEvents = ExploreEventsTestHelpers.CreateTestEvents(testSprite);

        // Act
        highlightsSubSectionComponent.SetLiveEvents(testEvents);

        // Assert
        Assert.AreEqual(2, highlightsSubSectionComponent.liveEvents.instantiatedItems.Count, "The number of set events does not match.");
        Assert.IsTrue(highlightsSubSectionComponent.liveEvents.instantiatedItems.Any(x => (x as EventCardComponentView).model == testEvents[0]), "The event 1 is not contained in the places grid");
        Assert.IsTrue(highlightsSubSectionComponent.liveEvents.instantiatedItems.Any(x => (x as EventCardComponentView).model == testEvents[1]), "The event 2 is not contained in the places grid");
        Assert.IsFalse(highlightsSubSectionComponent.liveEventsNoDataText.gameObject.activeSelf, "The liveEventsNoDataText should be visible.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetTrendingEventsAsLoadingCorrectly(bool isVisible)
    {
        // Arrange
        highlightsSubSectionComponent.liveEvents.gameObject.SetActive(isVisible);
        highlightsSubSectionComponent.liveEventsLoading.SetActive(!isVisible);

        // Act
        highlightsSubSectionComponent.SetLiveAsLoading(isVisible);

        // Assert
        Assert.AreEqual(!isVisible, highlightsSubSectionComponent.liveEvents.gameObject.activeSelf);
        Assert.AreEqual(isVisible, highlightsSubSectionComponent.liveEventsLoading.activeSelf);

        if (isVisible)
            Assert.IsFalse(highlightsSubSectionComponent.liveEventsNoDataText.gameObject.activeSelf);
    }

    [Test]
    public void ShowPlaceModalCorrectly()
    {
        // Arrange
        PlaceCardComponentModel testPlaceInfo = ExplorePlacesTestHelpers.CreateTestPlace("Test Place", testSprite);

        // Act
        highlightsSubSectionComponent.ShowPlaceModal(testPlaceInfo);

        // Assert
        Assert.AreEqual(testPlaceInfo, highlightsSubSectionComponent.placeModal.model, "The place modal model does not match.");

        highlightsSubSectionComponent.HidePlaceModal();
    }

    [Test]
    public void ShowEventModalCorrectly()
    {
        // Arrange
        EventCardComponentModel testEventInfo = ExploreEventsTestHelpers.CreateTestEvent("1", testSprite);

        // Act
        highlightsSubSectionComponent.ShowEventModal(testEventInfo);

        // Assert
        Assert.AreEqual(testEventInfo, highlightsSubSectionComponent.eventModal.model, "The event modal model does not match.");

        highlightsSubSectionComponent.HideEventModal();
    }
}