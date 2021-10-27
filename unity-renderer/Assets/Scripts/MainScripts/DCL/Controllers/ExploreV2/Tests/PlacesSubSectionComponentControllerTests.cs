using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using static HotScenesController;

public class PlacesSubSectionComponentControllerTests
{
    private PlacesSubSectionComponentController placesSubSectionComponentController;
    private IPlacesSubSectionComponentView placesSubSectionComponentView;
    private IPlacesAPIController placesAPIController;
    private IFriendsController friendsController;

    [SetUp]
    public void SetUp()
    {
        placesSubSectionComponentView = Substitute.For<IPlacesSubSectionComponentView>();
        placesAPIController = Substitute.For<IPlacesAPIController>();
        friendsController = Substitute.For<IFriendsController>();
        placesSubSectionComponentController = new PlacesSubSectionComponentController(placesSubSectionComponentView, placesAPIController, friendsController);
    }

    [TearDown]
    public void TearDown() { placesSubSectionComponentController.Dispose(); }

    [Test]
    public void InitializeCorrectly()
    {
        // Assert
        Assert.AreEqual(placesSubSectionComponentView, placesSubSectionComponentController.view);
        Assert.AreEqual(placesAPIController, placesSubSectionComponentController.placesAPIApiController);
        Assert.IsNotNull(placesSubSectionComponentController.friendsTrackerController);
    }

    [Test]
    public void DoFirstLoadingCorrectly()
    {
        // Arrange
        placesSubSectionComponentController.reloadPlaces = true;

        // Act
        placesSubSectionComponentController.FirstLoading();

        // Assert
        placesSubSectionComponentView.Received().RestartScrollViewPosition();
        placesSubSectionComponentView.Received().SetPlacesAsLoading(true);
        placesSubSectionComponentView.Received().SetShowMorePlacesButtonActive(false);
        Assert.IsFalse(placesSubSectionComponentController.reloadPlaces);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void RaiseOnExploreV2OpenCorrectly(bool isOpen)
    {
        // Arrange
        placesSubSectionComponentController.reloadPlaces = false;

        // Act
        placesSubSectionComponentController.OnExploreV2Open(isOpen, false);

        // Assert
        if (isOpen)
            Assert.IsFalse(placesSubSectionComponentController.reloadPlaces);
        else
            Assert.IsTrue(placesSubSectionComponentController.reloadPlaces);
    }

    [Test]
    public void RequestAllPlacesCorrectly()
    {
        // Arrange
        placesSubSectionComponentController.currentPlacesShowed = -1;
        placesSubSectionComponentController.reloadPlaces = true;

        // Act
        placesSubSectionComponentController.RequestAllPlaces();

        // Assert
        Assert.AreEqual(placesSubSectionComponentView.currentPlacesPerRow * PlacesSubSectionComponentController.INITIAL_NUMBER_OF_ROWS, placesSubSectionComponentController.currentPlacesShowed);
        placesSubSectionComponentView.Received().RestartScrollViewPosition();
        placesSubSectionComponentView.Received().SetPlacesAsLoading(true);
        placesSubSectionComponentView.Received().SetShowMorePlacesButtonActive(false);
        placesAPIController.Received().GetAllPlaces(Arg.Any<Action<List<HotSceneInfo>>>());
        Assert.IsFalse(placesSubSectionComponentController.reloadPlaces);
    }

    [Test]
    public void RequestAllPlacesFromAPICorrectly()
    {
        // Act
        placesSubSectionComponentController.RequestAllPlacesFromAPI();

        // Assert
        placesAPIController.Received().GetAllPlaces(Arg.Any<Action<List<HotSceneInfo>>>());
    }

    [Test]
    public void RaiseOnRequestedPlacesUpdatedCorrectly()
    {
        // Arrange
        int numberOfPlaces = 2;
        placesSubSectionComponentController.placesFromAPI = CreateTestPlacesFromApi(numberOfPlaces);

        // Act
        placesSubSectionComponentController.OnRequestedPlacesUpdated();

        // Assert
        placesSubSectionComponentView.Received().SetPlaces(Arg.Any<List<PlaceCardComponentModel>>());
        placesSubSectionComponentView.Received().SetShowMorePlacesButtonActive(placesSubSectionComponentController.currentPlacesShowed < numberOfPlaces);
        placesSubSectionComponentView.Received().SetPlacesAsLoading(false);
    }

    [Test]
    public void LoadPlacesCorrectly()
    {
        // Arrange
        int numberOfPlaces = 2;
        placesSubSectionComponentController.placesFromAPI = CreateTestPlacesFromApi(numberOfPlaces);

        // Act
        placesSubSectionComponentController.LoadPlaces();

        // Assert
        placesSubSectionComponentView.Received().SetPlaces(Arg.Any<List<PlaceCardComponentModel>>());
        placesSubSectionComponentView.Received().SetShowMorePlacesButtonActive(placesSubSectionComponentController.currentPlacesShowed < numberOfPlaces);
        placesSubSectionComponentView.Received().SetPlacesAsLoading(false);
    }

    [Test]
    [TestCase(2)]
    [TestCase(10)]
    public void LoaShowMorePlacesCorrectly(int numberOfPlaces)
    {
        // Act
        placesSubSectionComponentController.ShowMorePlaces();

        // Assert
        placesSubSectionComponentView.Received().SetShowMorePlacesButtonActive(Arg.Any<bool>());
    }

    [Test]
    public void CreatePlaceCardModelFromAPIPlaceCorrectly()
    {
        // Arrange
        HotSceneInfo testPlaceFromAPI = CreateTestHotSceneInfo("1");

        // Act
        PlaceCardComponentModel placeCardModel = placesSubSectionComponentController.CreatePlaceCardModelFromAPIPlace(testPlaceFromAPI);

        // Assert
        Assert.AreEqual(testPlaceFromAPI.thumbnail, placeCardModel.placePictureUri);
        Assert.AreEqual(testPlaceFromAPI.name, placeCardModel.placeName);
        Assert.AreEqual(placesSubSectionComponentController.FormatDescription(testPlaceFromAPI), placeCardModel.placeDescription);
        Assert.AreEqual(placesSubSectionComponentController.FormatAuthorName(testPlaceFromAPI), placeCardModel.placeAuthor);
        Assert.AreEqual(testPlaceFromAPI.usersTotalCount, placeCardModel.numberOfUsers);
        Assert.AreEqual(testPlaceFromAPI.parcels, placeCardModel.parcels);
        Assert.AreEqual(testPlaceFromAPI.baseCoords, placeCardModel.coords);
        Assert.AreEqual(testPlaceFromAPI, placeCardModel.hotSceneInfo);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void FormatDescriptionCorrectly(bool emptyDescription)
    {
        // Arrange
        HotSceneInfo testPlaceFromAPI = CreateTestHotSceneInfo("1");
        if (emptyDescription)
            testPlaceFromAPI.description = "";

        // Act
        string result = placesSubSectionComponentController.FormatDescription(testPlaceFromAPI);

        // Assert
        Assert.AreEqual(emptyDescription ? PlacesSubSectionComponentController.NO_PLACE_DESCRIPTION_WRITTEN : testPlaceFromAPI.description, result);
    }

    [Test]
    public void FormatAuthorNameCorrectly()
    {
        // Arrange
        HotSceneInfo testPlaceFromAPI = CreateTestHotSceneInfo("1");

        // Act
        string result = placesSubSectionComponentController.FormatAuthorName(testPlaceFromAPI);

        // Assert
        Assert.AreEqual($"Author <b>{testPlaceFromAPI.creator}</b>", result);
    }

    [Test]
    public void ShowPlaceDetailedInfoCorrectly()
    {
        // Arrange
        PlaceCardComponentModel testPlaceCardModel = new PlaceCardComponentModel();

        // Act
        placesSubSectionComponentController.ShowPlaceDetailedInfo(testPlaceCardModel);

        // Assert
        placesSubSectionComponentView.Received().ShowPlaceModal(testPlaceCardModel);
    }

    [Test]
    public void JumpInToPlaceCorrectly()
    {
        // Arrange
        bool exploreClosed = false;
        placesSubSectionComponentController.OnCloseExploreV2 += () => exploreClosed = true;
        HotSceneInfo testPlaceFromAPI = CreateTestHotSceneInfo("1");

        // Act
        placesSubSectionComponentController.JumpInToPlace(testPlaceFromAPI);

        // Assert
        placesSubSectionComponentView.Received().HidePlaceModal();
        Assert.IsTrue(exploreClosed);
    }

    private List<HotSceneInfo> CreateTestPlacesFromApi(int numberOfPlaces)
    {
        List<HotSceneInfo> testPlaces = new List<HotSceneInfo>();

        for (int i = 0; i < numberOfPlaces; i++)
        {
            testPlaces.Add(CreateTestHotSceneInfo((i + 1).ToString()));
        }

        return testPlaces;
    }

    private HotSceneInfo CreateTestHotSceneInfo(string id)
    {
        return new HotSceneInfo
        {
            id = id,
            baseCoords = new Vector2Int(10, 10),
            creator = "Test Creator",
            description = "Test Description",
            name = "Test Name",
            parcels = new Vector2Int[] { new Vector2Int(10, 10), new Vector2Int(20, 20) },
            realms = new HotSceneInfo.Realm[]
            {
                new HotSceneInfo.Realm
                {
                    layer = "Test Layer",
                    maxUsers = 500,
                    serverName = "Test Server",
                    userParcels = new Vector2Int[] { new Vector2Int(10, 10), new Vector2Int(20, 20) },
                    usersCount = 50
                }
            },
            thumbnail = "Test Thumbnail",
            usersTotalCount = 50
        };
    }
}