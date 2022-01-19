using DCL;
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
    void LoadPlaces();

    /// <summary>
    /// Increment the number of places loaded.
    /// </summary>
    void ShowMorePlaces();
}

public class PlacesSubSectionComponentController : IPlacesSubSectionComponentController
{
    public event Action OnCloseExploreV2;
    internal event Action OnPlacesFromAPIUpdated;

    internal const int INITIAL_NUMBER_OF_ROWS = 5;
    internal const int SHOW_MORE_ROWS_INCREMENT = 3;
    internal IPlacesSubSectionComponentView view;
    internal IPlacesAPIController placesAPIApiController;
    internal FriendTrackerController friendsTrackerController;
    internal List<HotSceneInfo> placesFromAPI = new List<HotSceneInfo>();
    internal int currentPlacesShowed = 0;
    internal bool reloadPlaces = false;
    internal IExploreV2Analytics exploreV2Analytics;

    public PlacesSubSectionComponentController(
        IPlacesSubSectionComponentView view,
        IPlacesAPIController placesAPI,
        IFriendsController friendsController,
        IExploreV2Analytics exploreV2Analytics)
    {
        this.view = view;
        this.view.OnReady += FirstLoading;
        this.view.OnInfoClicked += ShowPlaceDetailedInfo;
        this.view.OnJumpInClicked += JumpInToPlace;
        this.view.OnFriendHandlerAdded += View_OnFriendHandlerAdded;
        this.view.OnShowMorePlacesClicked += ShowMorePlaces;

        placesAPIApiController = placesAPI;
        OnPlacesFromAPIUpdated += OnRequestedPlacesUpdated;

        friendsTrackerController = new FriendTrackerController(friendsController, view.currentFriendColors);

        this.exploreV2Analytics = exploreV2Analytics;

        view.ConfigurePools();
    }

    internal void FirstLoading()
    {
        reloadPlaces = true;
        RequestAllPlaces();

        view.OnPlacesSubSectionEnable += RequestAllPlaces;
        DataStore.i.exploreV2.isOpen.OnChange += OnExploreV2Open;
    }

    internal void OnExploreV2Open(bool current, bool previous)
    {
        if (current)
            return;

        reloadPlaces = true;
    }

    public void RequestAllPlaces()
    {
        if (!reloadPlaces)
            return;

        currentPlacesShowed = view.currentPlacesPerRow * INITIAL_NUMBER_OF_ROWS;
        view.RestartScrollViewPosition();
        view.SetPlacesAsLoading(true);
        view.SetShowMorePlacesButtonActive(false);
        reloadPlaces = false;

        if (!DataStore.i.exploreV2.isInShowAnimationTransiton.Get())
            RequestAllPlacesFromAPI();
        else
            DataStore.i.exploreV2.isInShowAnimationTransiton.OnChange += IsInShowAnimationTransitonChanged;
    }

    internal void IsInShowAnimationTransitonChanged(bool current, bool previous)
    {
        DataStore.i.exploreV2.isInShowAnimationTransiton.OnChange -= IsInShowAnimationTransitonChanged;
        RequestAllPlacesFromAPI();
    }

    internal void RequestAllPlacesFromAPI()
    {
        placesAPIApiController.GetAllPlaces(
            (placeList) =>
            {
                placesFromAPI = placeList;
                OnPlacesFromAPIUpdated?.Invoke();
            });
    }

    internal void OnRequestedPlacesUpdated() { LoadPlaces(); }

    public void LoadPlaces()
    {
        friendsTrackerController.RemoveAllHandlers();

        List<PlaceCardComponentModel> places = new List<PlaceCardComponentModel>();
        List<HotSceneInfo> placesFiltered = placesFromAPI.Take(currentPlacesShowed).ToList();
        foreach (HotSceneInfo receivedPlace in placesFiltered)
        {
            PlaceCardComponentModel placeCardModel = ExplorePlacesUtils.CreatePlaceCardModelFromAPIPlace(receivedPlace);
            places.Add(placeCardModel);
        }

        view.SetPlaces(places);
        view.SetShowMorePlacesButtonActive(currentPlacesShowed < placesFromAPI.Count);
        view.SetPlacesAsLoading(false);
    }

    public void ShowMorePlaces()
    {
        List<PlaceCardComponentModel> places = new List<PlaceCardComponentModel>();
        List<HotSceneInfo> placesFiltered = new List<HotSceneInfo>();
        int numberOfExtraItemsToAdd = ((int)Mathf.Ceil((float)currentPlacesShowed / view.currentPlacesPerRow) * view.currentPlacesPerRow) - currentPlacesShowed;
        int numberOfItemsToAdd = view.currentPlacesPerRow * SHOW_MORE_ROWS_INCREMENT + numberOfExtraItemsToAdd;

        if (currentPlacesShowed + numberOfItemsToAdd <= placesFromAPI.Count)
            placesFiltered = placesFromAPI.GetRange(currentPlacesShowed, numberOfItemsToAdd);
        else
            placesFiltered = placesFromAPI.GetRange(currentPlacesShowed, placesFromAPI.Count - currentPlacesShowed);

        foreach (HotSceneInfo receivedPlace in placesFiltered)
        {
            PlaceCardComponentModel placeCardModel = ExplorePlacesUtils.CreatePlaceCardModelFromAPIPlace(receivedPlace);
            places.Add(placeCardModel);
        }

        view.AddPlaces(places);

        currentPlacesShowed += numberOfItemsToAdd;
        if (currentPlacesShowed > placesFromAPI.Count)
            currentPlacesShowed = placesFromAPI.Count;

        view.SetShowMorePlacesButtonActive(currentPlacesShowed < placesFromAPI.Count);
    }

    public void Dispose()
    {
        view.OnReady -= FirstLoading;
        view.OnInfoClicked -= ShowPlaceDetailedInfo;
        view.OnJumpInClicked -= JumpInToPlace;
        view.OnPlacesSubSectionEnable -= RequestAllPlaces;
        view.OnFriendHandlerAdded -= View_OnFriendHandlerAdded;
        view.OnShowMorePlacesClicked -= ShowMorePlaces;
        OnPlacesFromAPIUpdated -= OnRequestedPlacesUpdated;
        DataStore.i.exploreV2.isOpen.OnChange -= OnExploreV2Open;
    }

    internal void ShowPlaceDetailedInfo(PlaceCardComponentModel placeModel)
    {
        view.ShowPlaceModal(placeModel);
        exploreV2Analytics.SendClickOnPlaceInfo(placeModel.hotSceneInfo.id, placeModel.placeName);
    }

    internal void JumpInToPlace(HotSceneInfo placeFromAPI)
    {
        ExplorePlacesUtils.JumpInToPlace(placeFromAPI);
        view.HidePlaceModal();
        OnCloseExploreV2?.Invoke();
        exploreV2Analytics.SendPlaceTeleport(placeFromAPI.id, placeFromAPI.name, placeFromAPI.baseCoords);
    }

    internal void View_OnFriendHandlerAdded(FriendsHandler friendsHandler) { friendsTrackerController.AddHandler(friendsHandler); }
}