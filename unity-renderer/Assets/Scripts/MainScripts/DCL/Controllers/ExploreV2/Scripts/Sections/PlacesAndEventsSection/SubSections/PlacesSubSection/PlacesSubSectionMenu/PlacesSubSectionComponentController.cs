using DCL;
using DCL.Social.Friends;
using ExploreV2Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static HotScenesController;

public interface IPlacesSubSectionComponentController : IDisposable
{
    /// <summary>
    /// It will be triggered when the sub-section want to request to close the ExploreV2 main menu.
    /// </summary>
    event Action OnCloseExploreV2;

    /// <summary>
    /// Request all places from the API.
    /// </summary>
    void RequestAllPlaces();

    /// <summary>
    /// Load the places with the last requested ones.
    /// </summary>
    /// <param name="placeList"></param>
    void LoadPlaces(List<HotSceneInfo> placeList);

    /// <summary>
    /// Increment the number of places loaded.
    /// </summary>
    void ShowMorePlaces();
}

public class PlacesSubSectionComponentController : IPlacesSubSectionComponentController
{
    public event Action OnCloseExploreV2;

    internal const int INITIAL_NUMBER_OF_ROWS = 5;
    private const int SHOW_MORE_ROWS_INCREMENT = 3;

    internal readonly IPlacesSubSectionComponentView view;
    internal readonly IPlacesAPIController placesAPIApiController;
    internal readonly FriendTrackerController friendsTrackerController;
    private readonly IExploreV2Analytics exploreV2Analytics;
    private readonly DataStore dataStore;

    private readonly PlaceAndEventsCardsRequestHandler cardsRequestHandler;

    private List<HotSceneInfo> placesFromAPI = new ();
    private int currentCardsShown;

    public PlacesSubSectionComponentController(IPlacesSubSectionComponentView view, IPlacesAPIController placesAPI, IFriendsController friendsController, IExploreV2Analytics exploreV2Analytics, DataStore dataStore)
    {
        cardsRequestHandler = new PlaceAndEventsCardsRequestHandler(view, dataStore.exploreV2, RequestAllPlacesFromAPI);

        this.view = view;

        this.view.OnReady += FirstLoading;
        this.view.OnInfoClicked += ShowPlaceDetailedInfo;
        this.view.OnJumpInClicked += JumpInToPlace;
        this.view.OnFriendHandlerAdded += View_OnFriendHandlerAdded;
        this.view.OnShowMorePlacesClicked += ShowMorePlaces;

        this.dataStore = dataStore;
        this.dataStore.channels.currentJoinChannelModal.OnChange += OnChannelToJoinChanged;

        placesAPIApiController = placesAPI;

        friendsTrackerController = new FriendTrackerController(friendsController, view.currentFriendColors);

        this.exploreV2Analytics = exploreV2Analytics;

        view.ConfigurePools();
    }

    public void Dispose()
    {
        view.OnReady -= FirstLoading;
        view.OnInfoClicked -= ShowPlaceDetailedInfo;
        view.OnJumpInClicked -= JumpInToPlace;
        view.OnPlacesSubSectionEnable -= RequestAllPlaces;
        view.OnFriendHandlerAdded -= View_OnFriendHandlerAdded;
        view.OnShowMorePlacesClicked -= ShowMorePlaces;

        dataStore.channels.currentJoinChannelModal.OnChange -= OnChannelToJoinChanged;

        cardsRequestHandler.Dispose();
    }

    private void FirstLoading()
    {
        view.OnPlacesSubSectionEnable += RequestAllPlaces;
        cardsRequestHandler.Initialize();
    }

    public void RequestAllPlaces()
    {
        if (cardsRequestHandler.CanReload())
        {
            currentCardsShown = view.CurrentTilesPerRow * INITIAL_NUMBER_OF_ROWS;
            view.SetShowMoreButtonActive(false);

            cardsRequestHandler.RequestAll();
        }
    }

    internal void RequestAllPlacesFromAPI()
    {
        placesAPIApiController.GetAllPlaces(OnCompleted: LoadPlaces);
    }

    public void LoadPlaces(List<HotSceneInfo> placeList)
    {
        placesFromAPI = placeList;
        friendsTrackerController.RemoveAllHandlers();

        List<PlaceCardComponentModel> places = new List<PlaceCardComponentModel>();
        List<HotSceneInfo> placesFiltered = placesFromAPI.Take(currentCardsShown).ToList();

        foreach (HotSceneInfo receivedPlace in placesFiltered)
        {
            PlaceCardComponentModel placeCardModel = ExplorePlacesUtils.CreatePlaceCardModelFromAPIPlace(receivedPlace);
            places.Add(placeCardModel);
        }

        view.SetPlaces(places);
        view.SetShowMorePlacesButtonActive(currentCardsShown < placesFromAPI.Count);
    }

    public void ShowMorePlaces()
    {
        List<PlaceCardComponentModel> places = new List<PlaceCardComponentModel>();

        int numberOfExtraItemsToAdd = ((int)Mathf.Ceil((float)currentCardsShown / view.currentPlacesPerRow) * view.currentPlacesPerRow) - currentCardsShown;
        int numberOfItemsToAdd = (view.currentPlacesPerRow * SHOW_MORE_ROWS_INCREMENT) + numberOfExtraItemsToAdd;

        List<HotSceneInfo> placesFiltered = currentCardsShown + numberOfItemsToAdd <= placesFromAPI.Count
            ? placesFromAPI.GetRange(currentCardsShown, numberOfItemsToAdd)
            : placesFromAPI.GetRange(currentCardsShown, placesFromAPI.Count - currentCardsShown);

        foreach (HotSceneInfo receivedPlace in placesFiltered)
        {
            PlaceCardComponentModel placeCardModel = ExplorePlacesUtils.CreatePlaceCardModelFromAPIPlace(receivedPlace);
            places.Add(placeCardModel);
        }

        view.AddPlaces(places);

        currentCardsShown += numberOfItemsToAdd;

        if (currentCardsShown > placesFromAPI.Count)
            currentCardsShown = placesFromAPI.Count;

        view.SetShowMorePlacesButtonActive(currentCardsShown < placesFromAPI.Count);
    }

    internal void ShowPlaceDetailedInfo(PlaceCardComponentModel placeModel)
    {
        view.ShowPlaceModal(placeModel);
        exploreV2Analytics.SendClickOnPlaceInfo(placeModel.hotSceneInfo.id, placeModel.placeName);
        dataStore.exploreV2.currentVisibleModal.Set(ExploreV2CurrentModal.Places);
    }

    internal void JumpInToPlace(HotSceneInfo placeFromAPI)
    {
        ExplorePlacesUtils.JumpInToPlace(placeFromAPI);
        view.HidePlaceModal();
        dataStore.exploreV2.currentVisibleModal.Set(ExploreV2CurrentModal.None);
        OnCloseExploreV2?.Invoke();
        exploreV2Analytics.SendPlaceTeleport(placeFromAPI.id, placeFromAPI.name, placeFromAPI.baseCoords);
    }

    private void View_OnFriendHandlerAdded(FriendsHandler friendsHandler) =>
        friendsTrackerController.AddHandler(friendsHandler);

    private void OnChannelToJoinChanged(string currentChannelId, string previousChannelId)
    {
        if (!string.IsNullOrEmpty(currentChannelId))
            return;

        view.HidePlaceModal();
        dataStore.exploreV2.currentVisibleModal.Set(ExploreV2CurrentModal.None);
        OnCloseExploreV2?.Invoke();
    }
}
