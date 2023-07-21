using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using DCL.Social.Friends;
using DCLServices.PlacesAPIService;
using ExploreV2Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Environment = DCL.Environment;
using MainScripts.DCL.Controllers.HotScenes;
using System.Threading;
using static MainScripts.DCL.Controllers.HotScenes.IHotScenesController;

public class FavoritesesSubSectionComponentController : IFavoritesSubSectionComponentController, IPlacesAndEventsAPIRequester
{
    public event Action OnCloseExploreV2;

    internal const int INITIAL_NUMBER_OF_ROWS = 5;
    private const int SHOW_MORE_ROWS_INCREMENT = 3;

    internal readonly IFavoritesSubSectionComponentView view;
    internal readonly IPlacesAPIService placesAPIService;
    internal readonly FriendTrackerController friendsTrackerController;
    private readonly IExploreV2Analytics exploreV2Analytics;
    private readonly IPlacesAnalytics placesAnalytics;
    private readonly DataStore dataStore;

    internal readonly PlaceAndEventsCardsReloader cardsReloader;

    internal readonly List<PlaceInfo> favoritesFromAPI = new ();
    internal int availableUISlots;

    private CancellationTokenSource cts = new ();

    public FavoritesesSubSectionComponentController(
        IFavoritesSubSectionComponentView view,
        IPlacesAPIService placesAPI,
        IFriendsController friendsController,
        IExploreV2Analytics exploreV2Analytics,
        IPlacesAnalytics placesAnalytics,
        DataStore dataStore)
    {
        this.dataStore = dataStore;
        this.dataStore.channels.currentJoinChannelModal.OnChange += OnChannelToJoinChanged;
        this.view = view;
        this.exploreV2Analytics = exploreV2Analytics;
        this.placesAnalytics = placesAnalytics;
        placesAPIService = placesAPI;

        this.view.OnReady += FirstLoading;
        this.view.OnInfoClicked += ShowPlaceDetailedInfo;
        this.view.OnJumpInClicked += OnJumpInToPlace;
        this.view.OnShowMoreFavoritesClicked += ShowMoreFavorites;
        this.view.OnFriendHandlerAdded += View_OnFriendHandlerAdded;
        this.view.OnFavoriteClicked += View_OnFavoritesClicked;
        friendsTrackerController = new FriendTrackerController(friendsController, view.currentFriendColors);
        cardsReloader = new PlaceAndEventsCardsReloader(view, this, dataStore.exploreV2);

        view.ConfigurePools();
    }

    private void View_OnFavoritesClicked(string placeUUID, bool isFavorite)
    {
        if(isFavorite)
            placesAnalytics.AddFavorite(placeUUID, IPlacesAnalytics.ActionSource.FromExplore);
        else
            placesAnalytics.RemoveFavorite(placeUUID, IPlacesAnalytics.ActionSource.FromExplore);

        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
        placesAPIService.SetPlaceFavorite(placeUUID, isFavorite, cts.Token);
    }

    public void Dispose()
    {
        view.OnReady -= FirstLoading;
        view.OnInfoClicked -= ShowPlaceDetailedInfo;
        view.OnJumpInClicked -= OnJumpInToPlace;
        view.OnFavoriteSubSectionEnable -= RequestAllFavorites;
        view.OnFriendHandlerAdded -= View_OnFriendHandlerAdded;
        view.OnShowMoreFavoritesClicked -= ShowMoreFavorites;

        dataStore.channels.currentJoinChannelModal.OnChange -= OnChannelToJoinChanged;

        cardsReloader.Dispose();
        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
    }

    private void FirstLoading()
    {
        view.OnFavoriteSubSectionEnable += RequestAllFavorites;
        cardsReloader.Initialize();
    }

    internal void RequestAllFavorites()
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
        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
        GetAllFavoritesAsync(cts.Token).Forget();
    }

    private async UniTaskVoid GetAllFavoritesAsync(CancellationToken ct)
    {
        try
        {
            var favorites = await placesAPIService.GetFavorites(cts.Token);
            friendsTrackerController.RemoveAllHandlers();

            favoritesFromAPI.Clear();
            favoritesFromAPI.AddRange(favorites);

            view.SetFavorites(PlacesAndEventsCardsFactory.ConvertPlaceResponseToModel(favoritesFromAPI, availableUISlots));
            view.SetShowMoreFavoritesButtonActive(availableUISlots < favoritesFromAPI.Count);
        }
        catch (OperationCanceledException) { }
    }

    internal void ShowMoreFavorites()
    {
        int numberOfExtraItemsToAdd = ((int)Mathf.Ceil((float)availableUISlots / view.currentFavoritePlacesPerRow) * view.currentFavoritePlacesPerRow) - availableUISlots;
        int numberOfItemsToAdd = (view.currentFavoritePlacesPerRow * SHOW_MORE_ROWS_INCREMENT) + numberOfExtraItemsToAdd;

        List<PlaceInfo> placesFiltered = availableUISlots + numberOfItemsToAdd <= favoritesFromAPI.Count
            ? favoritesFromAPI.GetRange(availableUISlots, numberOfItemsToAdd)
            : favoritesFromAPI.GetRange(availableUISlots, favoritesFromAPI.Count - availableUISlots);

        view.AddFavorites(PlacesAndEventsCardsFactory.ConvertPlaceResponseToModel(placesFiltered));

        availableUISlots += numberOfItemsToAdd;

        if (availableUISlots > favoritesFromAPI.Count)
            availableUISlots = favoritesFromAPI.Count;

        view.SetShowMoreFavoritesButtonActive(availableUISlots < favoritesFromAPI.Count);
    }

    internal void ShowPlaceDetailedInfo(PlaceCardComponentModel placeModel)
    {
        view.ShowPlaceModal(placeModel);
        exploreV2Analytics.SendClickOnPlaceInfo(placeModel.placeInfo.id, placeModel.placeName);

        dataStore.exploreV2.currentVisibleModal.Set(ExploreV2CurrentModal.Places);
    }

    internal void OnJumpInToPlace(PlaceInfo placeFromAPI)
    {
        JumpInToPlace(placeFromAPI);
        view.HidePlaceModal();

        dataStore.exploreV2.currentVisibleModal.Set(ExploreV2CurrentModal.None);
        OnCloseExploreV2?.Invoke();
        exploreV2Analytics.SendPlaceTeleport(placeFromAPI.id, placeFromAPI.title, Utils.ConvertStringToVector(placeFromAPI.base_position));
        exploreV2Analytics.TeleportToPlaceFromFavorite(placeFromAPI.id, placeFromAPI.title);
    }

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
