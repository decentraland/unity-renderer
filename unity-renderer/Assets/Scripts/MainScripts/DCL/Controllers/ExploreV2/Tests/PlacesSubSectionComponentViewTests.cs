using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlacesSubSectionComponentViewTests
{
    private PlacesSubSectionComponentView placesSubSectionComponent;
    private Texture2D testTexture;
    private Sprite testSprite;

    [SetUp]
    public void SetUp()
    {
        placesSubSectionComponent = Object.Instantiate(Resources.Load<GameObject>("Sections/PlacesAndEventsSection/PlacesSubSection/PlacesSubSection")).GetComponent<PlacesSubSectionComponentView>();
        placesSubSectionComponent.ConfigurePools();
        placesSubSectionComponent.Start();
        testTexture = new Texture2D(20, 20);
        testSprite = Sprite.Create(testTexture, new Rect(), Vector2.zero);
    }

    [TearDown]
    public void TearDown()
    {
        placesSubSectionComponent.places.ExtractItems();
        placesSubSectionComponent.placeCardsPool.ReleaseAll();
        placesSubSectionComponent.Dispose();
        GameObject.Destroy(placesSubSectionComponent.placeModal.gameObject);
        GameObject.Destroy(placesSubSectionComponent.gameObject);
        GameObject.Destroy(testTexture);
        GameObject.Destroy(testSprite);
    }

    [Test]
    public void SetPlacesCorrectly()
    {
        // Arrange
        List<PlaceCardComponentModel> testPlaces = ExplorePlacesTestHelpers.CreateTestPlaces(testSprite);

        // Act
        placesSubSectionComponent.SetPlaces(testPlaces);

        // Assert
        Assert.AreEqual(2, placesSubSectionComponent.places.instantiatedItems.Count, "The number of set places does not match.");
        Assert.IsTrue(placesSubSectionComponent.places.instantiatedItems.Any(x => (x as PlaceCardComponentView).model == testPlaces[0]), "The place 1 is not contained in the places grid");
        Assert.IsTrue(placesSubSectionComponent.places.instantiatedItems.Any(x => (x as PlaceCardComponentView).model == testPlaces[1]), "The place 2 is not contained in the places grid");
        Assert.IsFalse(placesSubSectionComponent.placesNoDataText.gameObject.activeSelf, "The placesNoDataText should be visible.");
    }

    [Test]
    public void AddPlacesCorrectly()
    {
        // Arrange
        placesSubSectionComponent.places.RemoveItems();
        List<PlaceCardComponentModel> testPlaces = ExplorePlacesTestHelpers.CreateTestPlaces(testSprite);

        // Act
        placesSubSectionComponent.AddPlaces(testPlaces);

        // Assert
        Assert.AreEqual(2, placesSubSectionComponent.places.instantiatedItems.Count, "The number of set places does not match.");
        Assert.IsTrue(placesSubSectionComponent.places.instantiatedItems.Any(x => (x as PlaceCardComponentView).model == testPlaces[0]), "The place 1 is not contained in the places grid");
        Assert.IsTrue(placesSubSectionComponent.places.instantiatedItems.Any(x => (x as PlaceCardComponentView).model == testPlaces[1]), "The place 2 is not contained in the places grid");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetPlacesAsLoadingCorrectly(bool isVisible)
    {
        // Arrange
        placesSubSectionComponent.places.gameObject.SetActive(isVisible);
        placesSubSectionComponent.placesLoading.SetActive(!isVisible);
        placesSubSectionComponent.placesNoDataText.gameObject.SetActive(true);

        // Act
        placesSubSectionComponent.SetPlacesAsLoading(isVisible);

        // Assert
        Assert.AreEqual(!isVisible, placesSubSectionComponent.places.gameObject.activeSelf);
        Assert.AreEqual(isVisible, placesSubSectionComponent.placesLoading.activeSelf);

        if (isVisible)
            Assert.IsFalse(placesSubSectionComponent.placesNoDataText.gameObject.activeSelf);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetShowMorePlacesButtonActiveCorrectly(bool isVisible)
    {
        // Arrange
        placesSubSectionComponent.showMorePlacesButtonContainer.gameObject.SetActive(!isVisible);

        // Act
        placesSubSectionComponent.SetShowMorePlacesButtonActive(isVisible);

        // Assert
        Assert.AreEqual(isVisible, placesSubSectionComponent.showMorePlacesButtonContainer.gameObject.activeSelf);
    }

    [Test]
    public void ShowPlaceModalCorrectly()
    {
        // Arrange
        PlaceCardComponentModel testPlaceInfo = ExplorePlacesTestHelpers.CreateTestPlace("Test Place", testSprite);

        // Act
        placesSubSectionComponent.ShowPlaceModal(testPlaceInfo);

        // Assert
        Assert.AreEqual(testPlaceInfo, placesSubSectionComponent.placeModal.model, "The place modal model does not match.");

        placesSubSectionComponent.HidePlaceModal();
    }
}