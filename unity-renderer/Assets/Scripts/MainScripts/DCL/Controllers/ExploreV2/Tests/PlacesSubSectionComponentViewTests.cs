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
        GameObject.Destroy(placesSubSectionComponent.gameObject);
        GameObject.Destroy(testTexture);
        GameObject.Destroy(testSprite);
    }

    [Test]
    public void SetPlacesCorrectly()
    {
        // Arrange
        List<PlaceCardComponentModel> testPlaces = CreateTestPlaces();

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
        List<PlaceCardComponentModel> testPlaces = CreateTestPlaces();

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
        PlaceCardComponentModel testPlaceInfo = CreateTestPlace("Test Place");

        // Act
        placesSubSectionComponent.ShowPlaceModal(testPlaceInfo);

        // Assert
        Assert.AreEqual(testPlaceInfo, placesSubSectionComponent.placeModal.model, "The place modal model does not match.");

        placesSubSectionComponent.HidePlaceModal();
    }

    [Test]
    public void ConfigurePlaceCardModalCorrectly()
    {
        // Arrange
        placesSubSectionComponent.placeModal = null;

        // Act
        placesSubSectionComponent.ConfigurePlaceCardModal();

        // Assert
        Assert.IsNotNull(placesSubSectionComponent.placeModal);
    }

    [Test]
    public void ConfigurePlaceCardsPoolCorrectly()
    {
        // Arrange
        placesSubSectionComponent.placeCardsPool = null;

        // Act
        placesSubSectionComponent.ConfigurePlaceCardsPool();

        // Assert
        Assert.IsNotNull(placesSubSectionComponent.placeCardsPool);
        Assert.AreEqual(PlacesSubSectionComponentView.PLACE_CARDS_POOL_NAME, placesSubSectionComponent.placeCardsPool.id);
    }

    [Test]
    public void ConfigurePlaceCardCorrectly()
    {
        // Arrange
        PlaceCardComponentModel testPlaceInfo = CreateTestPlace("Test Place");

        // Act
        placesSubSectionComponent.ConfigurePlaceCard(placesSubSectionComponent.placeModal, testPlaceInfo);

        // Assert
        Assert.AreEqual(testPlaceInfo, placesSubSectionComponent.placeModal.model, "The place card model does not match.");
    }

    private List<PlaceCardComponentModel> CreateTestPlaces()
    {
        List<PlaceCardComponentModel> testPlaces = new List<PlaceCardComponentModel>();
        testPlaces.Add(CreateTestPlace("Test Place 1"));
        testPlaces.Add(CreateTestPlace("Test Place 2"));

        return testPlaces;
    }

    private PlaceCardComponentModel CreateTestPlace(string name)
    {
        return new PlaceCardComponentModel
        {
            coords = new Vector2Int(10, 10),
            hotSceneInfo = new HotScenesController.HotSceneInfo(),
            numberOfUsers = 10,
            parcels = new Vector2Int[] { new Vector2Int(10, 10), new Vector2Int(20, 20) },
            placeAuthor = "Test Author",
            placeDescription = "Test Description",
            placeName = name,
            placePictureSprite = testSprite
        };
    }
}