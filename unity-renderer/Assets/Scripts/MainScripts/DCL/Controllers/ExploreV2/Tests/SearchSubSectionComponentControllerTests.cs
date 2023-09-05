using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using DCLServices.PlacesAPIService;
using DCLServices.WorldsAPIService;
using ExploreV2Analytics;
using MainScripts.DCL.Controllers.HotScenes;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class SearchSubSectionComponentControllerTests
{
    private SearchSubSectionComponentController searchSubSectionComponentController;
    private ISearchSubSectionComponentView searchSubSectionComponentView;
    private SearchBarComponentView searchBarComponentView;
    private IEventsAPIController eventsAPIController;
    private IExploreV2Analytics exploreV2Analytics;
    private IPlacesAnalytics placesAnalytics;
    private IUserProfileBridge userProfileBridge;
    private IPlacesAPIService placesAPIService;
    private IWorldsAPIService worldsAPIService;

    [SetUp]
    public void SetUp()
    {
        // This is need to sue the TeleportController
        ServiceLocator serviceLocator = ServiceLocatorTestFactory.CreateMocked();
        DCL.Environment.Setup(serviceLocator);

        searchSubSectionComponentView = Substitute.For<ISearchSubSectionComponentView>();
        SearchBarComponentView searchBarPrefab = AssetDatabase.LoadAssetAtPath<SearchBarComponentView>("Assets/UIComponents/Prefabs/SearchBar.prefab");
        searchBarComponentView = Object.Instantiate(searchBarPrefab);
        eventsAPIController = Substitute.For<IEventsAPIController>();
        exploreV2Analytics = Substitute.For<IExploreV2Analytics>();
        placesAnalytics = Substitute.For<IPlacesAnalytics>();
        userProfileBridge = Substitute.For<IUserProfileBridge>();
        placesAPIService = Substitute.For<IPlacesAPIService>();
        worldsAPIService = Substitute.For<IWorldsAPIService>();
        var ownUserProfile = ScriptableObject.CreateInstance<UserProfile>();
        ownUserProfile.UpdateData(new UserProfileModel
        {
            userId = "ownId",
            hasConnectedWeb3 = true,
        });
        userProfileBridge.GetOwn().Returns(ownUserProfile);
        searchSubSectionComponentController = new SearchSubSectionComponentController(
            searchSubSectionComponentView,
            searchBarComponentView,
            eventsAPIController,
            placesAPIService,
            worldsAPIService,
            userProfileBridge,
            exploreV2Analytics,
            placesAnalytics,
            DataStore.i);
    }

    [TearDown]
    public void TearDown()
    {
        searchBarComponentView.Dispose();
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    [TestCase(null)]
    public void ChangeVoteCorrectly(bool? isUpvote)
    {
        // Arrange
        const string TEST_PLACE_ID = "testPlaceId";

        // Act
        searchSubSectionComponentView.OnVoteChanged += Raise.Event<Action<string, bool?>>(TEST_PLACE_ID, isUpvote);

        // Assert
        if (isUpvote != null)
        {
            if (isUpvote.Value)
                placesAnalytics.Received(1).Like(TEST_PLACE_ID, IPlacesAnalytics.ActionSource.FromSearch);
            else
                placesAnalytics.Received(1).Dislike(TEST_PLACE_ID, IPlacesAnalytics.ActionSource.FromSearch);
        }
        else
            placesAnalytics.Received(1).RemoveVote(TEST_PLACE_ID, IPlacesAnalytics.ActionSource.FromSearch);

        placesAPIService.Received(1).SetPlaceVote(isUpvote, TEST_PLACE_ID, default);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ChangePlaceFavoriteCorrectly(bool isFavorite)
    {
        // Arrange
        const string TEST_PLACE_ID = "testPlaceId";

        // Act
        searchSubSectionComponentView.OnPlaceFavoriteChanged += Raise.Event<Action<string, bool>>(TEST_PLACE_ID, isFavorite);

        // Assert
        if (isFavorite)
            placesAnalytics.Received(1).AddFavorite(TEST_PLACE_ID, IPlacesAnalytics.ActionSource.FromSearch);
        else
            placesAnalytics.Received(1).RemoveFavorite(TEST_PLACE_ID, IPlacesAnalytics.ActionSource.FromSearch);

        placesAPIService.SetPlaceFavorite(TEST_PLACE_ID, isFavorite, default);
    }

    [Test]
    public void JumpInToEventCorrectly()
    {
        // Arrange
        var onCloseExploreV2Triggered = false;
        searchSubSectionComponentController.OnCloseExploreV2 += () => onCloseExploreV2Triggered = true;
        const string TEST_EVENT_ID = "testEventId";
        const string TEST_EVENT_NAME = "testEventName";
        Vector2Int testEventCoors = new Vector2Int(100, 100);

        // Act
        searchSubSectionComponentView.OnEventJumpInClicked += Raise.Event<Action<EventFromAPIModel>>(new EventFromAPIModel
        {
            id = TEST_EVENT_ID,
            name = TEST_EVENT_NAME,
            coordinates = new[] {testEventCoors.x, testEventCoors.y},
        });

        // Assert
        Assert.IsTrue(onCloseExploreV2Triggered);
        exploreV2Analytics.Received(1).SendEventTeleport(TEST_EVENT_ID, TEST_EVENT_NAME, testEventCoors, ActionSource.FromSearch);
    }

    [Test]
    public void JumpInToPlaceCorrectly()
    {
        // Arrange
        var onCloseExploreV2Triggered = false;
        searchSubSectionComponentController.OnCloseExploreV2 += () => onCloseExploreV2Triggered = true;
        const string TEST_PLACE_ID = "testPlaceId";
        const string TEST_PLACE_NAME = "testPlaceName";
        const string TEST_PLACE_COORDS = "100,100";

        // Act
        searchSubSectionComponentView.OnPlaceJumpInClicked += Raise.Event<Action<IHotScenesController.PlaceInfo>>(new IHotScenesController.PlaceInfo
        {
            id = TEST_PLACE_ID,
            title = TEST_PLACE_NAME,
            base_position = TEST_PLACE_COORDS,
            realms_detail = new []
            {
                new IHotScenesController.PlaceInfo.Realm()
                {
                    layer = "testLayer1",
                    serverName = "testServer1",
                    usersCount = 1000,
                },
                new IHotScenesController.PlaceInfo.Realm()
                {
                    layer = "testLayer2",
                    serverName = "testServer2",
                    usersCount = 2000,
                },
            }
        });

        // Assert
        Assert.IsTrue(onCloseExploreV2Triggered);
        exploreV2Analytics.Received(1).SendPlaceTeleport(TEST_PLACE_ID, TEST_PLACE_NAME, Utils.ConvertStringToVector(TEST_PLACE_COORDS), ActionSource.FromSearch);
    }

    [Test]
    public void OpenEventDetailsModalCorrectly()
    {
        // Arrange
        const string TEST_EVENT_ID = "testEventId";
        const string TEST_EVENT_NAME = "testEventName";
        EventCardComponentModel testEventCardComponentModel = new EventCardComponentModel
        {
            eventId = TEST_EVENT_ID,
            eventName = TEST_EVENT_NAME,
        };
        var testIndex = 5;

        // Act
        searchSubSectionComponentView.OnEventInfoClicked += Raise.Event<Action<EventCardComponentModel, int>>(testEventCardComponentModel, testIndex);

        // Assert
        searchSubSectionComponentView.Received(1).ShowEventModal(testEventCardComponentModel);
        exploreV2Analytics.Received(1).SendClickOnEventInfo(TEST_EVENT_ID, TEST_EVENT_NAME, testIndex, ActionSource.FromSearch);
    }

    [Test]
    public void OpenPlaceDetailsModalCorrectly()
    {
        // Arrange
        const string TEST_PLACE_ID = "testPlaceId";
        const string TEST_PLACE_NAME = "testPlaceName";
        PlaceCardComponentModel testPlaceCardComponentModel = new PlaceCardComponentModel
        {
            placeInfo = new IHotScenesController.PlaceInfo { id = TEST_PLACE_ID },
            placeName = TEST_PLACE_NAME,
        };
        var testIndex = 5;

        // Act
        searchSubSectionComponentView.OnPlaceInfoClicked += Raise.Event<Action<PlaceCardComponentModel, int>>(testPlaceCardComponentModel, testIndex);

        // Assert
        searchSubSectionComponentView.Received(1).ShowPlaceModal(testPlaceCardComponentModel);
        exploreV2Analytics.Received(1).SendClickOnPlaceInfo(TEST_PLACE_ID, TEST_PLACE_NAME, testIndex, ActionSource.FromSearch);
    }

    [Test]
    public void SubscribeToEventCorrectly()
    {
        // Arrange
        const string TEST_EVENT_ID = "testEventId";

        // Act
        searchSubSectionComponentView.OnSubscribeEventClicked += Raise.Event<Action<string>>(TEST_EVENT_ID);

        // Assert
        exploreV2Analytics.Received(1).SendParticipateEvent(TEST_EVENT_ID, ActionSource.FromSearch);
        eventsAPIController.Received(1).RegisterParticipation(TEST_EVENT_ID);
    }

    [Test]
    public void UnsubscribeToEventCorrectly()
    {
        // Arrange
        const string TEST_EVENT_ID = "testEventId";

        // Act
        searchSubSectionComponentView.OnUnsubscribeEventClicked += Raise.Event<Action<string>>(TEST_EVENT_ID);

        // Assert
        eventsAPIController.Received(1).RemoveParticipation(TEST_EVENT_ID);
        exploreV2Analytics.Received(1).SendParticipateEvent(TEST_EVENT_ID, ActionSource.FromSearch);
    }

    [Test]
    public void SearchAllEventsCorrectly()
    {
        // Arrange
        const int NUMBER_OF_EVENTS = 2;
        List<EventFromAPIModel> testEventFromAPI = ExploreEventsTestHelpers.CreateTestEventsFromApi(NUMBER_OF_EVENTS);

        eventsAPIController
           .Configure()
           .SearchEvents(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
           .Returns(new UniTask<(List<EventFromAPIModel>, int total)>((testEventFromAPI, NUMBER_OF_EVENTS)));

        List<IHotScenesController.PlaceInfo> testPlaces = new ()
        {
            new IHotScenesController.PlaceInfo
            {
                id = "testId1",
                title = "place_1",
                Positions = new []{ new Vector2Int(1,1) },
            },
            new IHotScenesController.PlaceInfo
            {
                id = "testId2",
                title = "place_2",
                Positions = new []{ new Vector2Int(testEventFromAPI[0].coordinates[0],testEventFromAPI[0].coordinates[1]) },
            },
            new IHotScenesController.PlaceInfo
            {
                id = "testId3",
                title = "place_3",
                Positions = new []{ new Vector2Int(3,3) },
            },
        };

        placesAPIService.Configure()
                        .GetPlacesByCoordsList(Arg.Any<IEnumerable<Vector2Int>>(), Arg.Any<CancellationToken>())
                        .Returns(new UniTask<List<IHotScenesController.PlaceInfo>>(testPlaces));

        // Act
        searchSubSectionComponentView.OnRequestAllEvents += Raise.Event<Action<int>>(0);

        // Assert
        foreach (var testEvent in testEventFromAPI)
            Assert.AreEqual("place_2", testEvent.scene_name);

        exploreV2Analytics.Received(1).SendSearchEvents("", Arg.Any<Vector2Int[]>(), Arg.Any<string[]>());
        searchSubSectionComponentView.Received(1).ShowAllEvents(Arg.Any<List<EventCardComponentModel>>(), Arg.Any<bool>());
    }
}
