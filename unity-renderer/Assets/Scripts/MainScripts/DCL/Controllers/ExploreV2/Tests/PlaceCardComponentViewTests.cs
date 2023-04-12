using DCL.Helpers;
using MainScripts.DCL.Controllers.HotScenes;
using NSubstitute;
using NUnit.Framework;
using System.Linq;
using UnityEngine;

public class PlaceCardComponentViewTests
{
    private PlaceCardComponentView placeCardComponent;
    private PlaceCardComponentView placeCardModalComponent;
    private Texture2D testTexture;
    private Sprite testSprite;

    [SetUp]
    public void SetUp()
    {
        placeCardComponent = BaseComponentView.Create<PlaceCardComponentView>("Sections/PlacesAndEventsSection/PlacesSubSection/PlaceCard");
        placeCardModalComponent = BaseComponentView.Create<PlaceCardComponentView>("Sections/PlacesAndEventsSection/PlacesSubSection/PlaceCard_Modal");
        placeCardComponent.placeImage.imageObserver = Substitute.For<ILazyTextureObserver>();
        placeCardModalComponent.placeImage.imageObserver = Substitute.For<ILazyTextureObserver>();
        testTexture = new Texture2D(20, 20);
        testSprite = Sprite.Create(testTexture, new Rect(), Vector2.zero);
    }

    [TearDown]
    public void TearDown()
    {
        placeCardComponent.Dispose();
        placeCardModalComponent.Dispose();
        GameObject.Destroy(placeCardComponent.gameObject);
        GameObject.Destroy(placeCardModalComponent.gameObject);
        GameObject.Destroy(testTexture);
        GameObject.Destroy(testSprite);
    }

    [Test]
    public void ConfigurePlaceCardCorrectly()
    {
        // Arrange
        PlaceCardComponentModel testModel = new PlaceCardComponentModel
        {
            coords = new Vector2Int(10, 10),
            hotSceneInfo = new IHotScenesController.HotSceneInfo(),
            numberOfUsers = 10,
            parcels = new Vector2Int[] { new Vector2Int(10, 10), new Vector2Int(20, 20) },
            placeAuthor = "Test Author",
            placeDescription = "Test Description",
            placeName = "Test Name",
            placePictureSprite = testSprite
        };

        // Act
        placeCardComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, placeCardComponent.model, "The model does not match after configuring the place card.");
    }

    [Test]
    public void RaiseOnFocusCorrectly()
    {
        // Arrange
        placeCardComponent.cardSelectionFrame.SetActive(false);

        // Act
        placeCardComponent.OnFocus();

        // Assert
        Assert.IsTrue(placeCardComponent.cardSelectionFrame.activeSelf, "The card selection frame should be activated.");
    }

    [Test]
    public void RaiseOnLoseFocusCorrectly()
    {
        // Arrange
        placeCardComponent.cardSelectionFrame.SetActive(true);

        // Act
        placeCardComponent.OnLoseFocus();

        // Assert
        Assert.IsFalse(placeCardComponent.cardSelectionFrame.activeSelf, "The card selection frame should be deactivated.");
    }

    [Test]
    public void SetPlaceCardPictureFromSpriteCorrectly()
    {
        // Act
        placeCardComponent.SetPlacePicture(testSprite);

        // Assert
        Assert.AreEqual(testSprite, placeCardComponent.model.placePictureSprite, "The place card picture sprite does not match in the model.");
        Assert.AreEqual(testSprite, placeCardComponent.placeImage.image.sprite, "The place card image does not match.");
    }

    [Test]
    public void SetPlaceCardPictureFromTextureCorrectly()
    {
        // Arrange
        placeCardComponent.model.placePictureTexture = null;

        // Act
        placeCardComponent.SetPlacePicture(testTexture);

        // Assert
        Assert.AreEqual(testTexture, placeCardComponent.model.placePictureTexture, "The place card picture texture does not match in the model.");
        placeCardComponent.placeImage.imageObserver.Received().RefreshWithTexture(testTexture);
    }

    [Test]
    public void SetPlaceCardPictureFromUriCorrectly()
    {
        // Arrange
        string testUri = "testUri";
        placeCardComponent.model.placePictureUri = null;

        // Act
        placeCardComponent.SetPlacePicture(testUri);

        // Assert
        Assert.AreEqual(testUri, placeCardComponent.model.placePictureUri, "The place card picture uri does not match in the model.");
        placeCardComponent.placeImage.imageObserver.Received().RefreshWithUri(testUri);
    }

    [Test]
    public void SetPlaceNameCorrectly()
    {
        // Arrange
        string testName = "Test Name";

        // Act
        placeCardComponent.SetPlaceName(testName);

        // Assert
        Assert.AreEqual(testName, placeCardComponent.model.placeName, "The place card name does not match in the model.");
        Assert.AreEqual(testName, placeCardComponent.placeNameOnIdleText.text);
        Assert.AreEqual(testName, placeCardComponent.placeNameOnFocusText.text);
    }

    [Test]
    public void SetPlaceDescriptionCorrectly()
    {
        // Arrange
        string testDescription = "Test Description";

        // Act
        placeCardModalComponent.SetPlaceDescription(testDescription);

        // Assert
        Assert.AreEqual(testDescription, placeCardModalComponent.model.placeDescription, "The place card description does not match in the model.");
        Assert.AreEqual(testDescription, placeCardModalComponent.placeDescText.text);
    }

    [Test]
    public void SetPlaceAuthorCorrectly()
    {
        // Arrange
        string testAuthor = "Test Author";

        // Act
        placeCardModalComponent.SetPlaceAuthor(testAuthor);

        // Assert
        Assert.AreEqual(testAuthor, placeCardModalComponent.model.placeAuthor, "The place card author does not match in the model.");
        Assert.AreEqual(testAuthor, placeCardModalComponent.placeAuthorText.text);
    }

    [Test]
    public void SetNumberOfUsersCorrectly()
    {
        // Arrange
        int testNumberOfUsers = 10;

        // Act
        placeCardComponent.SetNumberOfUsers(testNumberOfUsers);

        // Assert
        Assert.AreEqual(testNumberOfUsers, placeCardComponent.model.numberOfUsers, "The place card number of users does not match in the model.");
        Assert.AreEqual(testNumberOfUsers.ToString(), placeCardComponent.numberOfUsersText.text);
    }

    [Test]
    public void SetCoordsCorrectly()
    {
        // Arrange
        Vector2Int testCoords = new Vector2Int(10, 10);

        // Act
        placeCardModalComponent.SetCoords(testCoords);

        // Assert
        Assert.AreEqual(testCoords, placeCardModalComponent.model.coords, "The place card coords does not match in the model.");
        Assert.AreEqual($"{testCoords.x},{testCoords.y}", placeCardModalComponent.coordsText.text);
    }

    [Test]
    public void SetParcelsCorrectly()
    {
        // Arrange
        Vector2Int[] testParcels = new Vector2Int[] { new Vector2Int(10, 10), new Vector2Int(20, 20) };

        // Act
        placeCardComponent.SetParcels(testParcels);

        // Assert
        Assert.AreEqual(testParcels, placeCardComponent.model.parcels, "The place card parcels does not match in the model.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetLoadingIndicatorVisibleCorrectly(bool isVisible)
    {
        // Arrange
        placeCardComponent.imageContainer.SetActive(isVisible);
        placeCardComponent.placeInfoContainer.SetActive(isVisible);
        placeCardComponent.loadingSpinner.SetActive(!isVisible);

        // Act
        placeCardComponent.SetLoadingIndicatorVisible(isVisible);

        // Assert
        Assert.AreEqual(!isVisible, placeCardComponent.imageContainer.activeSelf, "The image container active property does not match.");
        Assert.AreEqual(!isVisible, placeCardComponent.placeInfoContainer.activeSelf, "The info container active property does not match.");
        Assert.AreEqual(isVisible, placeCardComponent.loadingSpinner.activeSelf, "The loading spinner active property does not match.");
    }

    [Test]
    public void InitializeFriendsTrackerCorrectly()
    {
        // Arrange
        placeCardComponent.mapInfoHandler = null;
        placeCardComponent.friendsHandler = null;

        // Act
        placeCardComponent.InitializeFriendsTracker();

        // Assert
        Assert.IsNotNull(placeCardComponent.mapInfoHandler, "The map info handler shouldn't be null");
        Assert.IsNotNull(placeCardComponent.friendsHandler, "The friends handler shouldn't be null");
    }

    [Test]
    public void RaiseOnFriendAddedCorrectly()
    {
        // Arrange
        string testUserId = "123456";
        UserProfile testProfile = new UserProfile();
        testProfile.UpdateData(new UserProfileModel { userId = testUserId });
        Color testColor = Color.green;
        placeCardComponent.currentFriendHeads.Clear();

        // Act
        placeCardComponent.OnFriendAdded(testProfile, testColor);

        // Assert
        Assert.IsTrue(placeCardComponent.friendsGrid.instantiatedItems.Any(x => (x as FriendHeadForPlaceCardComponentView).model.userProfile.userId == testUserId));
        Assert.IsTrue(placeCardComponent.currentFriendHeads.ContainsKey(testUserId));
    }

    [Test]
    public void RaiseOnFriendRemovedCorrectly()
    {
        // Arrange
        string testUserId = "123456";
        UserProfile testProfile = new UserProfile();
        testProfile.UpdateData(new UserProfileModel { userId = testUserId });
        Color testColor = Color.green;
        placeCardComponent.currentFriendHeads.Clear();
        placeCardComponent.OnFriendAdded(testProfile, testColor);

        // Act
        placeCardComponent.OnFriendRemoved(testProfile);

        // Assert
        Assert.IsFalse(placeCardComponent.friendsGrid.instantiatedItems.Any(x => (x as FriendHeadForPlaceCardComponentView).model.userProfile.userId == testUserId));
        Assert.IsFalse(placeCardComponent.currentFriendHeads.ContainsKey(testUserId));
    }

    [Test]
    public void CleanFriendHeadsItemsCorrectly()
    {
        // Arrange
        RaiseOnFriendAddedCorrectly();

        // Act
        placeCardComponent.CleanFriendHeadsItems();

        // Assert
        Assert.AreEqual(0, placeCardComponent.friendsGrid.instantiatedItems.Count, "The friends grid should be empty.");
        Assert.AreEqual(0, placeCardComponent.currentFriendHeads.Count, "The currentFriendHeads list should be empty.");
    }

    [Test]
    public void InstantiateAndConfigureFriendHeadCorrectly()
    {
        // Arrange
        FriendHeadForPlaceCardComponentModel testFriendInfo = new FriendHeadForPlaceCardComponentModel
        {
            userProfile = new UserProfile(),
            backgroundColor = Color.green
        };

        // Act
        BaseComponentView newFriendHead = placeCardComponent.InstantiateAndConfigureFriendHead(testFriendInfo, placeCardComponent.friendHeadPrefab);

        // Assert
        Assert.IsTrue(newFriendHead is FriendHeadForPlaceCardComponentView);
        Assert.AreEqual(testFriendInfo, ((FriendHeadForPlaceCardComponentView)newFriendHead).model, "The new friend head model does not match.");

        placeCardComponent.friendsGrid.Dispose();
    }

    [Test]
    public void CloseModalCorrectly()
    {
        // Arrange
        placeCardModalComponent.Show();

        // Act
        placeCardModalComponent.CloseModal();

        // Assert
        Assert.IsFalse(placeCardModalComponent.isVisible);
    }

    [Test]
    public void RaiseOnCloseActionTriggeredCorrectly()
    {
        // Arrange
        placeCardModalComponent.Show();

        // Act
        placeCardModalComponent.OnCloseActionTriggered(new DCLAction_Trigger());

        // Assert
        Assert.IsFalse(placeCardModalComponent.isVisible);
    }
}
