using DCL;
using DCL.Social.Friends;
using ExploreV2Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static HotScenesController;

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

    internal readonly PlaceAndEventsCardsReloader cardsReloader;

    internal List<HotSceneInfo> placesFromAPI = new ();
    internal int availableUISlots;

    public PlacesSubSectionComponentController(IPlacesSubSectionComponentView view, IPlacesAPIController placesAPI, IFriendsController friendsController, IExploreV2Analytics exploreV2Analytics, DataStore dataStore)
    {
        cardsReloader = new PlaceAndEventsCardsReloader(view, dataStore.exploreV2, RequestAllPlacesFromAPI);

        this.view = view;

        this.view.OnReady += FirstLoading;

        this.view.OnInfoClicked += ShowPlaceDetailedInfo;
        this.view.OnJumpInClicked += JumpInToPlace;

        this.view.OnShowMorePlacesClicked += ShowMorePlaces;

        this.view.OnFriendHandlerAdded += View_OnFriendHandlerAdded;

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

        cardsReloader.Dispose();
    }

    private void FirstLoading()
    {
        view.OnPlacesSubSectionEnable += RequestAllPlaces;
        cardsReloader.Initialize();
    }

    internal void RequestAllPlaces()
    {
        if (cardsReloader.CanReload())
        {
            availableUISlots = view.CurrentTilesPerRow * INITIAL_NUMBER_OF_ROWS;
            view.SetShowMoreButtonActive(false);

            cardsReloader.RequestAll();
        }
    }

    internal void RequestAllPlacesFromAPI()
    {
        placesAPIApiController.GetAllPlaces(
            OnCompleted: OnRequestedEventsUpdated);
    }

    private void OnRequestedEventsUpdated(List<HotSceneInfo> placeList)
    {
        friendsTrackerController.RemoveAllHandlers();

        placesFromAPI = placeList;

        view.SetPlaces(PlacesAndEventsCardsFactory.CreatePlacesCards((TakeAllForAvailableSlots(placeList))));

        view.SetShowMorePlacesButtonActive(availableUISlots < placesFromAPI.Count);
    }

    internal List<HotSceneInfo> TakeAllForAvailableSlots(List<HotSceneInfo> modelsFromAPI) =>
        modelsFromAPI.Take(availableUISlots).ToList();

    internal void ShowMorePlaces()
    {
        int numberOfExtraItemsToAdd = ((int)Mathf.Ceil((float)availableUISlots / view.currentPlacesPerRow) * view.currentPlacesPerRow) - availableUISlots;
        int numberOfItemsToAdd = (view.currentPlacesPerRow * SHOW_MORE_ROWS_INCREMENT) + numberOfExtraItemsToAdd;

        List<HotSceneInfo> placesFiltered = availableUISlots + numberOfItemsToAdd <= placesFromAPI.Count
            ? placesFromAPI.GetRange(availableUISlots, numberOfItemsToAdd)
            : placesFromAPI.GetRange(availableUISlots, placesFromAPI.Count - availableUISlots);

        view.AddPlaces(PlacesAndEventsCardsFactory.CreatePlacesCards(placesFiltered));

        availableUISlots += numberOfItemsToAdd;

        if (availableUISlots > placesFromAPI.Count)
            availableUISlots = placesFromAPI.Count;

        view.SetShowMorePlacesButtonActive(availableUISlots < placesFromAPI.Count);
    }

    internal void ShowPlaceDetailedInfo(PlaceCardComponentModel placeModel)
    {
        view.ShowPlaceModal(placeModel);
        exploreV2Analytics.SendClickOnPlaceInfo(placeModel.hotSceneInfo.id, placeModel.placeName);

        dataStore.exploreV2.currentVisibleModal.Set(ExploreV2CurrentModal.Places);
    }

    internal void JumpInToPlace(HotSceneInfo placeFromAPI)
    {
        PlacesCardsConfigurator.JumpInToPlace(placeFromAPI);
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
