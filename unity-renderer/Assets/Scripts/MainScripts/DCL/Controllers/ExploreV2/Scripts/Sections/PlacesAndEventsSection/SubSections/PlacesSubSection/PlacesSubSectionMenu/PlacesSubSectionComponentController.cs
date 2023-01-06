using DCL;
using DCL.Social.Friends;
using ExploreV2Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static HotScenesController;

public class CardsLoader<TModel, TInfo> where TModel: BaseComponentModel
{
    public List<TModel> LoadModelsFromInfo(List<TInfo> cardsInfoFromAPI, int amountToLoad, Func<TInfo, TModel> factoryCreateMethod)
    {
        List<TInfo> filteredCardsInfosToLoad = cardsInfoFromAPI.Take(amountToLoad).ToList();

        List<TModel> loadedCards = new List<TModel>();

        foreach (TInfo filteredCardInfo in filteredCardsInfosToLoad)
            loadedCards.Add(factoryCreateMethod(filteredCardInfo));

        return loadedCards;
    }
}

public class PlacesSubSectionComponentController : IPlacesSubSectionComponentController
{
    public event Action OnCloseExploreV2;

    private const int INITIAL_NUMBER_OF_ROWS = 5;
    private const int SHOW_MORE_ROWS_INCREMENT = 3;

    internal readonly IPlacesSubSectionComponentView view;
    internal readonly IPlacesAPIController placesAPIApiController;
    internal readonly FriendTrackerController friendsTrackerController;
    private readonly IExploreV2Analytics exploreV2Analytics;
    private readonly DataStore dataStore;

    private readonly PlaceAndEventsCardsRequestHandler cardsRequestHandler;
    private readonly CardsLoader<PlaceCardComponentModel, HotSceneInfo> cardsLoader;

    private List<HotSceneInfo> placesFromAPI = new ();
    private int availableUISlots;
    public PlacesSubSectionComponentController(IPlacesSubSectionComponentView view, IPlacesAPIController placesAPI, IFriendsController friendsController, IExploreV2Analytics exploreV2Analytics, DataStore dataStore)
    {
        cardsRequestHandler = new PlaceAndEventsCardsRequestHandler(view, dataStore.exploreV2, RequestAllPlacesFromAPI);
        cardsLoader = new CardsLoader<PlaceCardComponentModel, HotSceneInfo>();

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
            availableUISlots = view.CurrentTilesPerRow * INITIAL_NUMBER_OF_ROWS;
            view.SetShowMoreButtonActive(false);

            cardsRequestHandler.RequestAll();
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

        view.SetPlaces(
            cardsLoader.LoadModelsFromInfo(placesFromAPI, availableUISlots, ExplorePlacesUtils.CreatePlaceCardModelFromAPIPlace)
            );

        view.SetShowMorePlacesButtonActive(availableUISlots < placesFromAPI.Count);
    }

    public void ShowMorePlaces()
    {
        List<PlaceCardComponentModel> places = new List<PlaceCardComponentModel>();

        int numberOfExtraItemsToAdd = ((int)Mathf.Ceil((float)availableUISlots / view.currentPlacesPerRow) * view.currentPlacesPerRow) - availableUISlots;
        int numberOfItemsToAdd = (view.currentPlacesPerRow * SHOW_MORE_ROWS_INCREMENT) + numberOfExtraItemsToAdd;

        List<HotSceneInfo> placesFiltered = availableUISlots + numberOfItemsToAdd <= placesFromAPI.Count
            ? placesFromAPI.GetRange(availableUISlots, numberOfItemsToAdd)
            : placesFromAPI.GetRange(availableUISlots, placesFromAPI.Count - availableUISlots);

        foreach (HotSceneInfo receivedPlace in placesFiltered)
        {
            PlaceCardComponentModel placeCardModel = ExplorePlacesUtils.CreatePlaceCardModelFromAPIPlace(receivedPlace);
            places.Add(placeCardModel);
        }

        view.AddPlaces(places);

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
