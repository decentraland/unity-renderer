using NUnit.Framework;
using UnityEngine;

public class PlaceCardHelpersTests
{
    private PlacesSubSectionComponentView placesSubSectionComponent;
    private PlaceCardComponentView testPlaceCard;
    private Texture2D testTexture;
    private Sprite testSprite;

    [SetUp]
    public void SetUp()
    {
        placesSubSectionComponent = Object.Instantiate(Resources.Load<GameObject>("Sections/PlacesAndEventsSection/PlacesSubSection/PlacesSubSection")).GetComponent<PlacesSubSectionComponentView>();
        placesSubSectionComponent.Start();

        testPlaceCard = Object.Instantiate(Resources.Load<GameObject>("Sections/PlacesAndEventsSection/PlacesSubSection/PlaceCard_Modal")).GetComponent<PlaceCardComponentView>();

        testTexture = new Texture2D(20, 20);
        testSprite = Sprite.Create(testTexture, new Rect(), Vector2.zero);
    }

    [TearDown]
    public void TearDown()
    {
        placesSubSectionComponent.places.ExtractItems();
        placesSubSectionComponent.placeCardsPool.ReleaseAll();
        placesSubSectionComponent.Dispose();
        testPlaceCard.Dispose();
        GameObject.Destroy(placesSubSectionComponent.placeModal.gameObject);
        GameObject.Destroy(placesSubSectionComponent.gameObject);
        GameObject.Destroy(testPlaceCard.gameObject);
        GameObject.Destroy(testTexture);
        GameObject.Destroy(testSprite);
    }

    [Test]
    public void ConfigurePlaceCardModalCorrectly()
    {
        // Arrange
        GameObject.Destroy(placesSubSectionComponent.placeModal);
        placesSubSectionComponent.placeModal = null;

        // Act
        placesSubSectionComponent.placeModal = PlaceCardHelpers.ConfigurePlaceCardModal(placesSubSectionComponent.placeCardModalPrefab);

        // Assert
        Assert.IsNotNull(placesSubSectionComponent.placeModal);
        Assert.AreEqual(PlaceCardHelpers.PLACE_CARD_MODAL_ID, placesSubSectionComponent.placeModal.gameObject.name);
    }

    [Test]
    public void ConfigurePlaceCardsPoolCorrectly()
    {
        // Arrange
        placesSubSectionComponent.placeCardsPool = null;

        // Act
        PlaceCardHelpers.ConfigurePlaceCardsPool(
            out placesSubSectionComponent.placeCardsPool,
            PlacesSubSectionComponentView.PLACE_CARDS_POOL_NAME,
            placesSubSectionComponent.placeCardPrefab,
            200);

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
        PlaceCardHelpers.ConfigurePlaceCard(testPlaceCard, testPlaceInfo, null, null);

        // Assert
        Assert.AreEqual(testPlaceInfo, testPlaceCard.model, "The place card model does not match.");
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