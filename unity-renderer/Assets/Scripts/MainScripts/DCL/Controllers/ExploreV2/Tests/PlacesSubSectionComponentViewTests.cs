using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using static UnityEngine.Object;

public class PlacesSubSectionComponentViewTests
{
    private static int[] spritesAmounts = { 0, 1, 5, 6 };

    private PlacesSubSectionComponentView placesSubSectionComponent;
    private Texture2D testTexture;
    private Sprite testSprite;

    [SetUp]
    public void SetUp()
    {
        placesSubSectionComponent = Instantiate(Resources.Load<GameObject>("Sections/PlacesAndEventsSection/PlacesSubSection/PlacesSubSection")).GetComponent<PlacesSubSectionComponentView>();
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

        Destroy(testTexture);
        Destroy(testSprite);
        Destroy(placesSubSectionComponent.gameObject);

        if (placesSubSectionComponent.placeModal != null)
            Destroy(placesSubSectionComponent.placeModal.gameObject);
    }

    [UnityTest]
    public IEnumerator SetPlacesCorrectly([ValueSource(nameof(spritesAmounts))] int spritesAmount)
    {
        // Arrange
        placesSubSectionComponent.places.RemoveItems();
        placesSubSectionComponent.places.gameObject.SetActive(false); // hack needed for fix AABB canvas fails on tests (happens only in the tests)

        List<PlaceCardComponentModel> testPlaces = ExplorePlacesTestHelpers.CreateTestPlaces(testSprite, spritesAmount);

        // Act
        placesSubSectionComponent.SetPlaces(testPlaces);

        for (int i = 0; i < spritesAmount - 1; i++)
            yield return null;

        // Assert
        Assert.AreEqual(spritesAmount, placesSubSectionComponent.places.instantiatedItems.Count, "The number of set places does not match.");

        for (int i = 0; i < spritesAmount; i++)
            Assert.IsTrue(placesSubSectionComponent.places.instantiatedItems.Any(x => (x as PlaceCardComponentView)?.model == testPlaces[i]), $"The place {i} is not contained in the places grid");

        Assert.AreEqual(spritesAmount == 0, placesSubSectionComponent.placesNoDataText.gameObject.activeSelf, "The placesNoDataText should not be visible when there are places.");
    }

    [UnityTest]
    public IEnumerator AddPlacesCorrectly([ValueSource(nameof(spritesAmounts))] int spritesAmount)
    {
        // Arrange
        placesSubSectionComponent.places.RemoveItems();
        placesSubSectionComponent.places.gameObject.SetActive(false); // hack needed for fix AABB canvas fails on tests (happens only in the tests)
        List<PlaceCardComponentModel> testPlaces = ExplorePlacesTestHelpers.CreateTestPlaces(testSprite, spritesAmount);

        // Act
        placesSubSectionComponent.AddPlaces(testPlaces);

        for (int i = 0; i < spritesAmount - 1; i++)
            yield return null;

        // Assert
        Assert.AreEqual(spritesAmount, placesSubSectionComponent.places.instantiatedItems.Count, "The number of set places does not match.");

        for (int i = 0; i < spritesAmount; i++)
            Assert.IsTrue(placesSubSectionComponent.places.instantiatedItems.Any(x => (x as PlaceCardComponentView)?.model == testPlaces[i]), "The place 1 is not contained in the places grid");
    }

    [TestCase(true)]
    [TestCase(false)]
    public void SetPlacesAsLoadingCorrectly(bool isVisible)
    {
        // Arrange
        placesSubSectionComponent.places.gameObject.GetComponent<Canvas>().enabled = isVisible;
        placesSubSectionComponent.placesLoading.SetActive(!isVisible);
        placesSubSectionComponent.placesNoDataText.gameObject.SetActive(true);

        // Act
        placesSubSectionComponent.SetPlacesAsLoading(isVisible);

        // Assert
        Assert.AreEqual(!isVisible, placesSubSectionComponent.places.GetComponent<Canvas>().enabled);
        Assert.AreEqual(isVisible, placesSubSectionComponent.placesLoading.activeSelf);

        if (isVisible)
            Assert.IsFalse(placesSubSectionComponent.placesNoDataText.gameObject.activeSelf);
    }

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