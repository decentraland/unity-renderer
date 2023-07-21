using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using DCL.Social.Friends;
using DCL.Tasks;
using DCLServices.PlacesAPIService;
using ExploreV2Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MainScripts.DCL.Controllers.HotScenes;
using System.Threading;
using Environment = DCL.Environment;
using static MainScripts.DCL.Controllers.HotScenes.IHotScenesController;

public class PlacesSubSectionComponentController : IPlacesSubSectionComponentController, IPlacesAndEventsAPIRequester
{
    public event Action OnCloseExploreV2;

    internal const int INITIAL_NUMBER_OF_ROWS = 5;
    private const int PAGE_SIZE = 12;

    internal readonly IPlacesSubSectionComponentView view;
    internal readonly IPlacesAPIService placesAPIService;
    internal readonly FriendTrackerController friendsTrackerController;
    private readonly IExploreV2Analytics exploreV2Analytics;
    private readonly IPlacesAnalytics placesAnalytics;
    private readonly DataStore dataStore;

    internal readonly PlaceAndEventsCardsReloader cardsReloader;

    internal readonly List<PlaceInfo> placesFromAPI = new ();
    internal int availableUISlots;
    private CancellationTokenSource getPlacesCts = new ();
    private CancellationTokenSource showMoreCts = new ();
    private CancellationTokenSource disposeCts = new ();

    public PlacesSubSectionComponentController(
        IPlacesSubSectionComponentView view,
        IPlacesAPIService placesAPI,
        IFriendsController friendsController,
        IExploreV2Analytics exploreV2Analytics,
        IPlacesAnalytics placesAnalytics,
        DataStore dataStore)
    {
        cardsReloader = new PlaceAndEventsCardsReloader(view, this, dataStore.exploreV2);

        this.view = view;

        this.view.OnReady += FirstLoading;

        this.view.OnInfoClicked += ShowPlaceDetailedInfo;
        this.view.OnJumpInClicked += OnJumpInToPlace;
        this.view.OnFavoriteClicked += View_OnFavoritesClicked;

        this.view.OnShowMorePlacesClicked += ShowMorePlaces;

        this.view.OnFriendHandlerAdded += View_OnFriendHandlerAdded;

        this.dataStore = dataStore;
        this.dataStore.channels.currentJoinChannelModal.OnChange += OnChannelToJoinChanged;

        placesAPIService = placesAPI;

        friendsTrackerController = new FriendTrackerController(friendsController, view.currentFriendColors);

        this.exploreV2Analytics = exploreV2Analytics;
        this.placesAnalytics = placesAnalytics;

        view.ConfigurePools();
    }

    public void Dispose()
    {
        disposeCts?.SafeCancelAndDispose();
        showMoreCts?.SafeCancelAndDispose();
        getPlacesCts?.SafeCancelAndDispose();

        view.OnReady -= FirstLoading;
        view.OnInfoClicked -= ShowPlaceDetailedInfo;
        view.OnJumpInClicked -= OnJumpInToPlace;
        this.view.OnFavoriteClicked += View_OnFavoritesClicked;
        view.OnPlacesSubSectionEnable -= RequestAllPlaces;
        view.OnFriendHandlerAdded -= View_OnFriendHandlerAdded;
        view.OnShowMorePlacesClicked -= ShowMorePlaces;

        dataStore.channels.currentJoinChannelModal.OnChange -= OnChannelToJoinChanged;

        cardsReloader.Dispose();
    }

    private void View_OnFavoritesClicked(string placeUUID, bool isFavorite)
    {
        if(isFavorite)
            placesAnalytics.AddFavorite(placeUUID, IPlacesAnalytics.ActionSource.FromExplore);
        else
            placesAnalytics.RemoveFavorite(placeUUID, IPlacesAnalytics.ActionSource.FromExplore);

        placesAPIService.SetPlaceFavorite(placeUUID, isFavorite, disposeCts.Token);
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
        getPlacesCts?.SafeCancelAndDispose();
        getPlacesCts = CancellationTokenSource.CreateLinkedTokenSource(disposeCts.Token);

        RequestAllFromAPIAsync(getPlacesCts.Token).Forget();
    }

    private async UniTaskVoid RequestAllFromAPIAsync(CancellationToken ct)
    {
        try
        {
            (IReadOnlyList<PlaceInfo> places, int total) firstPage = await placesAPIService.GetMostActivePlaces(0, PAGE_SIZE, ct);
            friendsTrackerController.RemoveAllHandlers();
            placesFromAPI.Clear();
            placesFromAPI.AddRange(firstPage.places);
            if (firstPage.total > PAGE_SIZE)
            {
                (IReadOnlyList<PlaceInfo> places, int total) secondPage = await placesAPIService.GetMostActivePlaces(1, PAGE_SIZE, ct);
                placesFromAPI.AddRange(secondPage.places);
            }

            view.SetPlaces(PlacesAndEventsCardsFactory.ConvertPlaceResponseToModel(placesFromAPI, availableUISlots));

            view.SetShowMorePlacesButtonActive(placesFromAPI.Count < firstPage.total);
        }
        catch (OperationCanceledException) { }
    }

    internal void ShowMorePlaces()
    {
        showMoreCts?.SafeCancelAndDispose();
        showMoreCts = CancellationTokenSource.CreateLinkedTokenSource(disposeCts.Token);
        ShowMorePlacesAsync(showMoreCts.Token).Forget();
    }

    private async UniTask ShowMorePlacesAsync(CancellationToken ct)
    {
        (IReadOnlyList<PlaceInfo> places, int total) = await placesAPIService.GetMostActivePlaces((placesFromAPI.Count/PAGE_SIZE), PAGE_SIZE, showMoreCts.Token);

        placesFromAPI.AddRange(places);
        view.AddPlaces(PlacesAndEventsCardsFactory.ConvertPlaceResponseToModel(places));
        view.SetShowMorePlacesButtonActive(placesFromAPI.Count < total);
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
