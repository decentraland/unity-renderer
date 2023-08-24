using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using DCL.Social.Friends;
using DCL.Tasks;
using DCLServices.PlacesAPIService;
using DCLServices.WorldsAPIService;
using ExploreV2Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading;
using Environment = DCL.Environment;
using static MainScripts.DCL.Controllers.HotScenes.IHotScenesController;

public class WorldsSubSectionComponentController : IWorldsSubSectionComponentController, IPlacesAndEventsAPIRequester
{
    public event Action OnCloseExploreV2;

    internal const int INITIAL_NUMBER_OF_ROWS = 4;
    private const int PAGE_SIZE = 12;
    private const string ONLY_POI_FILTER = "only_pois=true";
    private const string MOST_ACTIVE_FILTER_ID = "most_active";

    internal readonly IWorldsSubSectionComponentView view;
    internal readonly IWorldsAPIService worldsAPIService;
    internal readonly IPlacesAPIService placesAPI;
    internal readonly FriendTrackerController friendsTrackerController;
    private readonly IExploreV2Analytics exploreV2Analytics;
    private readonly IPlacesAnalytics placesAnalytics;
    private readonly DataStore dataStore;
    private readonly IUserProfileBridge userProfileBridge;
    private IReadOnlyList<string> allPointOfInterest;

    internal readonly PlaceAndEventsCardsReloader cardsReloader;

    internal readonly List<WorldsResponse.WorldInfo> worldsFromAPI = new ();
    internal int availableUISlots;
    private CancellationTokenSource getWorldsCts = new ();
    private CancellationTokenSource showMoreCts = new ();
    private CancellationTokenSource disposeCts = new ();

    public WorldsSubSectionComponentController(
        IWorldsSubSectionComponentView view,
        IPlacesAPIService placesAPI,
        IWorldsAPIService worldsAPIService,
        IFriendsController friendsController,
        IExploreV2Analytics exploreV2Analytics,
        IPlacesAnalytics placesAnalytics,
        DataStore dataStore,
        IUserProfileBridge userProfileBridge)
    {
        cardsReloader = new PlaceAndEventsCardsReloader(view, this, dataStore.exploreV2);

        this.view = view;

        this.view.OnReady += FirstLoading;

        this.view.OnInfoClicked += ShowWorldDetailedInfo;
        this.view.OnJumpInClicked += OnJumpInToWorld;
        this.view.OnFavoriteClicked += View_OnFavoritesClicked;
        this.view.OnVoteChanged += View_OnVoteChanged;
        this.view.OnShowMoreWorldsClicked += ShowMoreWorlds;

        this.view.OnFriendHandlerAdded += View_OnFriendHandlerAdded;

        this.dataStore = dataStore;
        this.dataStore.channels.currentJoinChannelModal.OnChange += OnChannelToJoinChanged;

        this.placesAPI = placesAPI;
        this.worldsAPIService = worldsAPIService;

        friendsTrackerController = new FriendTrackerController(friendsController, view.currentFriendColors);

        this.exploreV2Analytics = exploreV2Analytics;
        this.placesAnalytics = placesAnalytics;

        this.userProfileBridge = userProfileBridge;

        view.ConfigurePools();
    }

    public void Dispose()
    {
        disposeCts?.SafeCancelAndDispose();
        showMoreCts?.SafeCancelAndDispose();
        getWorldsCts?.SafeCancelAndDispose();

        view.OnReady -= FirstLoading;
        view.OnInfoClicked -= ShowWorldDetailedInfo;
        view.OnJumpInClicked -= OnJumpInToWorld;
        view.OnFavoriteClicked -= View_OnFavoritesClicked;
        this.view.OnVoteChanged -= View_OnVoteChanged;
        view.OnWorldsSubSectionEnable -= OpenTab;
        view.OnSortingChanged -= ApplySorting;
        view.OnFriendHandlerAdded -= View_OnFriendHandlerAdded;
        view.OnShowMoreWorldsClicked -= ShowMoreWorlds;

        dataStore.channels.currentJoinChannelModal.OnChange -= OnChannelToJoinChanged;

        cardsReloader.Dispose();
    }

    private void View_OnVoteChanged(string placeId, bool? isUpvote)
    {
        if (userProfileBridge.GetOwn().isGuest)
            dataStore.HUDs.connectWalletModalVisible.Set(true);
        else
        {
            if (isUpvote != null)
            {
                if (isUpvote.Value)
                    placesAnalytics.Like(placeId, IPlacesAnalytics.ActionSource.FromExplore);
                else
                    placesAnalytics.Dislike(placeId, IPlacesAnalytics.ActionSource.FromExplore);
            }
            else
                placesAnalytics.RemoveVote(placeId, IPlacesAnalytics.ActionSource.FromExplore);

            placesAPI.SetPlaceVote(isUpvote, placeId, disposeCts.Token);
        }
    }

    private void View_OnFavoritesClicked(string placeUUID, bool isFavorite)
    {
        if (userProfileBridge.GetOwn().isGuest)
            dataStore.HUDs.connectWalletModalVisible.Set(true);
        else
        {
            if (isFavorite)
                placesAnalytics.AddFavorite(placeUUID, IPlacesAnalytics.ActionSource.FromExplore);
            else
                placesAnalytics.RemoveFavorite(placeUUID, IPlacesAnalytics.ActionSource.FromExplore);

            placesAPI.SetPlaceFavorite(placeUUID, isFavorite, disposeCts.Token);
        }
    }

    private void FirstLoading()
    {
        view.OnWorldsSubSectionEnable += OpenTab;
        view.OnSortingChanged += ApplySorting;
        cardsReloader.Initialize();
    }

    private void OpenTab()
    {
        exploreV2Analytics.SendWorldsTabOpen();
        RequestAllWorlds();
    }

    private void ApplySorting()
    {
        placesAnalytics.Sort(view.sort == MOST_ACTIVE_FILTER_ID ? IPlacesAnalytics.SortingType.MostActive : IPlacesAnalytics.SortingType.Best);
        RequestAllWorlds();
    }

    internal void RequestAllWorlds()
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
        getWorldsCts?.SafeCancelAndDispose();
        getWorldsCts = CancellationTokenSource.CreateLinkedTokenSource(disposeCts.Token);

        RequestAllFromAPIAsync(getWorldsCts.Token).Forget();
    }

    private async UniTaskVoid RequestAllFromAPIAsync(CancellationToken ct)
    {
        try
        {
            allPointOfInterest = await placesAPI.GetPointsOfInterestCoords(ct);
            if (allPointOfInterest != null)
                view.SetPOICoords(allPointOfInterest.ToList());

            string filter = view.filter;
            if (filter == ONLY_POI_FILTER)
                filter = BuildPointOfInterestFilter();

            (IReadOnlyList<WorldsResponse.WorldInfo> worlds, int total) firstPage = await worldsAPIService.GetWorlds(0, PAGE_SIZE, filter, view.sort, ct);
            friendsTrackerController.RemoveAllHandlers();
            worldsFromAPI.Clear();
            worldsFromAPI.AddRange(firstPage.worlds);
            if (firstPage.total > PAGE_SIZE)
            {
                (IReadOnlyList<WorldsResponse.WorldInfo> worlds, int total) secondPage = await worldsAPIService.GetWorlds(1, PAGE_SIZE, filter, view.sort, ct);
                worldsFromAPI.AddRange(secondPage.worlds);
            }

            view.SetWorlds(PlacesAndEventsCardsFactory.ConvertWorldsResponseToModel(worldsFromAPI, availableUISlots));
            view.SetShowMoreWorldsButtonActive(worldsFromAPI.Count < firstPage.total);
        }
        catch (OperationCanceledException) { }
    }

    internal void ShowMoreWorlds()
    {
        showMoreCts?.SafeCancelAndDispose();
        showMoreCts = CancellationTokenSource.CreateLinkedTokenSource(disposeCts.Token);
        ShowMoreWorldsAsync(showMoreCts.Token).Forget();
    }

    private async UniTask ShowMoreWorldsAsync(CancellationToken ct)
    {
        string filter = view.filter;
        if (filter == ONLY_POI_FILTER)
            filter = BuildPointOfInterestFilter();

        (IReadOnlyList<WorldsResponse.WorldInfo> worlds, int total) = await worldsAPIService.GetWorlds((worldsFromAPI.Count/PAGE_SIZE), PAGE_SIZE, filter, view.sort, showMoreCts.Token);

        worldsFromAPI.AddRange(worlds);
        view.AddWorlds(PlacesAndEventsCardsFactory.ConvertWorldsResponseToModel(worlds));
        view.SetShowMoreWorldsButtonActive(worldsFromAPI.Count < total);
    }

    internal void ShowWorldDetailedInfo(PlaceCardComponentModel worldModel)
    {
        view.ShowWorldModal(worldModel);
        exploreV2Analytics.SendClickOnPlaceInfo(worldModel.placeInfo.id, worldModel.placeName);

        dataStore.exploreV2.currentVisibleModal.Set(ExploreV2CurrentModal.Places);
    }

    internal void OnJumpInToWorld(PlaceInfo placeFromAPI)
    {
        JumpInToWorld(placeFromAPI);
        view.HideWorldModal();

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

        view.HideWorldModal();
        dataStore.exploreV2.currentVisibleModal.Set(ExploreV2CurrentModal.None);
        OnCloseExploreV2?.Invoke();
    }

    /// <summary>
    /// Makes a jump in to the world defined by the given world data from API.
    /// </summary>
    /// <param name="worldFromAPI">World data from API.</param>
    public static void JumpInToWorld(PlaceInfo worldFromAPI)
    {
        PlaceInfo.Realm realm = new PlaceInfo.Realm() { layer = null, serverName = null };
        worldFromAPI.realms_detail = worldFromAPI.realms_detail.OrderByDescending(x => x.usersCount).ToArray();

        for (int i = 0; i < worldFromAPI.realms_detail.Length; i++)
        {
            bool isArchipelagoRealm = string.IsNullOrEmpty(worldFromAPI.realms_detail[i].layer);

            if (isArchipelagoRealm || worldFromAPI.realms_detail[i].usersCount < worldFromAPI.realms_detail[i].maxUsers)
            {
                realm = worldFromAPI.realms_detail[i];
                break;
            }
        }

        Vector2Int position = Utils.ConvertStringToVector(worldFromAPI.base_position);

        //Environment.i.world.teleportController.JumpInWorld(placeFromAPI.world_name);
    }

    private string BuildPointOfInterestFilter()
    {
        string resultFilter = string.Empty;

        if (allPointOfInterest == null)
            return resultFilter;

        foreach (string poi in allPointOfInterest)
        {
            string x = poi.Split(",")[0];
            string y = poi.Split(",")[1];
            resultFilter = string.Concat(resultFilter, $"&positions={x},{y}");
        }

        return resultFilter;
    }
}
