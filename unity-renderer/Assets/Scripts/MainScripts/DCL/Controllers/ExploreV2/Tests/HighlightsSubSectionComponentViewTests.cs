using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using static UnityEngine.GameObject;
using static UnityEngine.Object;

public class HighlightsSubSectionComponentViewTests
{
    private static int[] spritesAmounts = { 0, 1, 5, 6 };

    private HighlightsSubSectionComponentView highlightsSubSectionComponent;
    private Texture2D testTexture;
    private Sprite testSprite;

    [SetUp]
    public void SetUp()
    {
        highlightsSubSectionComponent = Instantiate(Resources.Load<GameObject>("Sections/PlacesAndEventsSection/HighlightsSubSection/HighlightsSubSection")).GetComponent<HighlightsSubSectionComponentView>();
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

        if (highlightsSubSectionComponent.placeModal != null)
            Destroy(highlightsSubSectionComponent.placeModal.gameObject);

        if (highlightsSubSectionComponent.eventModal != null)
            Destroy(highlightsSubSectionComponent.eventModal.gameObject);

        Destroy(highlightsSubSectionComponent.gameObject);
        Destroy(testTexture);
        Destroy(testSprite);
    }

    [UnityTest]
    public IEnumerator SetPromotedPlacesAndEventsCorrectly()
    {
        // Arrange
        highlightsSubSectionComponent.gameObject.SetActive(false); // hack needed for fix AABB canvas fails on tests (happens only in the tests)
        List<PlaceCardComponentModel> testPlaces = ExplorePlacesTestHelpers.CreateTestPlaces(testSprite);
        List<EventCardComponentModel> testEvents = ExploreEventsTestHelpers.CreateTestEvents(testSprite);

        // Act
        highlightsSubSectionComponent.SetTrendingPlacesAndEvents(testPlaces, testEvents);

        for (int i = 0; i < testPlaces.Count + testEvents.Count - 1; i++)
            yield return null;

        // Assert
        Assert.AreEqual(testPlaces.Count + testEvents.Count, highlightsSubSectionComponent.trendingPlacesAndEvents.instantiatedItems.Count, "The number of set places/events does not match.");

        Assert.IsTrue(highlightsSubSectionComponent.trendingPlacesAndEvents.instantiatedItems.Any(x => (x is EventCardComponentView view) && view.model == testEvents[0]), "The event 1 is not contained in the carousel");
        Assert.IsTrue(highlightsSubSectionComponent.trendingPlacesAndEvents.instantiatedItems.Any(x => (x is EventCardComponentView view) && view.model == testEvents[1]), "The event 2 is not contained in the carousel");
        Assert.IsTrue(highlightsSubSectionComponent.trendingPlacesAndEvents.instantiatedItems.Any(x => (x is PlaceCardComponentView view) && view.model == testPlaces[0]), "The place 1 is not contained in the carousel");
        Assert.IsTrue(highlightsSubSectionComponent.trendingPlacesAndEvents.instantiatedItems.Any(x => (x is PlaceCardComponentView view) && view.model == testPlaces[1]), "The place 2 is not contained in the carousel");

        Assert.IsTrue(highlightsSubSectionComponent.trendingPlacesAndEvents.gameObject.activeSelf, "The promotedPlaces section should be visible.");
    }

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

    [UnityTest]
    public IEnumerator SetFeaturedPlacesCorrectly([ValueSource(nameof(spritesAmounts))] int spritesAmount)
    {
        // Arrange
        highlightsSubSectionComponent.gameObject.SetActive(false); // hack needed for fix AABB canvas fails on tests (happens only in the tests)
        List<PlaceCardComponentModel> testPlaces = ExplorePlacesTestHelpers.CreateTestPlaces(testSprite, spritesAmount);

        // Act
        highlightsSubSectionComponent.SetFeaturedPlaces(testPlaces);

        for (int i = 0; i < spritesAmount - 1; i++)
            yield return null;

        // Assert
        Assert.AreEqual(spritesAmount, highlightsSubSectionComponent.featuredPlaces.instantiatedItems.Count, "The number of set places does not match.");

        for (int i = 0; i < spritesAmount; i++)
            Assert.IsTrue(highlightsSubSectionComponent.featuredPlaces.instantiatedItems.Any(x => (x as PlaceCardComponentView)?.model == testPlaces[i]), $"The place {i} is not contained in the places grid");

        Assert.AreEqual(spritesAmount == 0, highlightsSubSectionComponent.featuredPlacesNoDataText.gameObject.activeSelf, "The featuredPlacesNoDataText should be visible.");
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

    [UnityTest]
    public IEnumerator SetLiveEventsCorrectly([ValueSource(nameof(spritesAmounts))] int spritesAmount)
    {
        // Arrange
        highlightsSubSectionComponent.gameObject.SetActive(false); // hack needed for fix AABB canvas fails on tests (happens only in the tests)
        List<EventCardComponentModel> testEvents = ExploreEventsTestHelpers.CreateTestEvents(testSprite, spritesAmount);

        // Act
        highlightsSubSectionComponent.SetLiveEvents(testEvents);

        for (int i = 0; i < spritesAmount - 1; i++)
            yield return null;

        // Assert
        Assert.AreEqual(spritesAmount, highlightsSubSectionComponent.liveEvents.instantiatedItems.Count, "The number of set events does not match.");

        for (int i = 0; i < spritesAmount; i++)
            Assert.IsTrue(highlightsSubSectionComponent.liveEvents.instantiatedItems.Any(x => (x as EventCardComponentView)?.model == testEvents[i]), $"The event {i} is not contained in the places grid");

        Assert.AreEqual(spritesAmount == 0, highlightsSubSectionComponent.liveEventsNoDataText.gameObject.activeSelf, "The liveEventsNoDataText should be visible.");
    }

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