using DCL;
using DCL.Helpers;
using DCL.Social.Friends;
using ExploreV2Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static HotScenesController;
using Environment = DCL.Environment;

public class PlacesSubSectionComponentController : IPlacesSubSectionComponentController, IPlacesAndEventsAPIRequester
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

    internal List<PlaceInfo> placesFromAPI = new ();
    internal int availableUISlots;

    public PlacesSubSectionComponentController(IPlacesSubSectionComponentView view, IPlacesAPIController placesAPI, IFriendsController friendsController, IExploreV2Analytics exploreV2Analytics, DataStore dataStore)
    {
        cardsReloader = new PlaceAndEventsCardsReloader(view, this, dataStore.exploreV2);

        this.view = view;

        this.view.OnReady += FirstLoading;

        this.view.OnInfoClicked += ShowPlaceDetailedInfo;
        this.view.OnJumpInClicked += OnJumpInToPlace;

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
        view.OnJumpInClicked -= OnJumpInToPlace;
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

    public void RequestAllFromAPI()
    {
        placesAPIApiController.GetAllPlacesFromPlacesAPI(
            OnCompleted: OnRequestedEventsUpdated);
    }

    private void OnRequestedEventsUpdated(List<PlaceInfo> placeList)
    {
        friendsTrackerController.RemoveAllHandlers();

        placesFromAPI = placeList;

        view.SetPlaces(PlacesAndEventsCardsFactory.ConvertPlaceResponseToModel(TakeAllForAvailableSlots(placeList)));

        view.SetShowMorePlacesButtonActive(availableUISlots < placesFromAPI.Count);
    }

    internal List<PlaceInfo> TakeAllForAvailableSlots(List<PlaceInfo> modelsFromAPI) =>
        modelsFromAPI.Take(availableUISlots).ToList();

    internal void ShowMorePlaces()
    {
        int numberOfExtraItemsToAdd = ((int)Mathf.Ceil((float)availableUISlots / view.currentPlacesPerRow) * view.currentPlacesPerRow) - availableUISlots;
        int numberOfItemsToAdd = (view.currentPlacesPerRow * SHOW_MORE_ROWS_INCREMENT) + numberOfExtraItemsToAdd;

        List<PlaceInfo> placesFiltered = availableUISlots + numberOfItemsToAdd <= placesFromAPI.Count
            ? placesFromAPI.GetRange(availableUISlots, numberOfItemsToAdd)
            : placesFromAPI.GetRange(availableUISlots, placesFromAPI.Count - availableUISlots);

        view.AddPlaces(PlacesAndEventsCardsFactory.ConvertPlaceResponseToModel(placesFiltered));

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

    internal void OnJumpInToPlace(PlaceInfo placeFromAPI)
    {
        JumpInToPlace(placeFromAPI);
        view.HidePlaceModal();

        dataStore.exploreV2.currentVisibleModal.Set(ExploreV2CurrentModal.None);
        OnCloseExploreV2?.Invoke();
        exploreV2Analytics.SendPlaceTeleport(placeFromAPI.id, placeFromAPI.title, Utils.ConvertStringToVector(placeFromAPI.base_position));
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

    /// <summary>
    /// Makes a jump in to the place defined by the given place data from API.
    /// </summary>
    /// <param name="placeFromAPI">Place data from API.</param>
    public static void JumpInToPlace(PlaceInfo placeFromAPI)
    {
        PlaceInfo.Realm realm = new PlaceInfo.Realm() { layer = null, serverName = null };
        placeFromAPI.realms_detail = placeFromAPI.realms_detail.OrderByDescending(x => x.usersCount).ToArray();

        for (int i = 0; i < placeFromAPI.realms_detail.Length; i++)
        {
            bool isArchipelagoRealm = string.IsNullOrEmpty(placeFromAPI.realms_detail[i].layer);

            if (isArchipelagoRealm || placeFromAPI.realms_detail[i].usersCount < placeFromAPI.realms_detail[i].maxUsers)
            {
                realm = placeFromAPI.realms_detail[i];
                break;
            }
        }

        Vector2Int position = Utils.ConvertStringToVector(placeFromAPI.base_position);

        if (string.IsNullOrEmpty(realm.serverName))
            Environment.i.world.teleportController.Teleport(position.x, position.y);
        else
            Environment.i.world.teleportController.JumpIn(position.x, position.y, realm.serverName, realm.layer);
    }
}
