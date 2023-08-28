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
    private const string MOST_ACTIVE_FILTER_ID = "most_active";

    internal readonly IWorldsSubSectionComponentView view;
    internal readonly IWorldsAPIService worldsAPIService;
    internal readonly IPlacesAPIService placesAPI;
    internal readonly FriendTrackerController friendsTrackerController;
    private readonly IExploreV2Analytics exploreV2Analytics;
    private readonly IPlacesAnalytics placesAnalytics;
    private readonly DataStore dataStore;
    private readonly IUserProfileBridge userProfileBridge;

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
        view.OnVoteChanged -= View_OnVoteChanged;
        view.OnWorldsSubSectionEnable -= OpenTab;
        view.OnSortingChanged -= ApplySorting;
        view.OnFriendHandlerAdded -= View_OnFriendHandlerAdded;
        view.OnShowMoreWorldsClicked -= ShowMoreWorlds;

        dataStore.channels.currentJoinChannelModal.OnChange -= OnChannelToJoinChanged;

        cardsReloader.Dispose();
    }

    private void View_OnVoteChanged(string worldId, bool? isUpvote)
    {
        if (userProfileBridge.GetOwn().isGuest)
            dataStore.HUDs.connectWalletModalVisible.Set(true);
        else
        {
            if (isUpvote != null)
            {
                if (isUpvote.Value)
                    placesAnalytics.Like(worldId, IPlacesAnalytics.ActionSource.FromExplore, true);
                else
                    placesAnalytics.Dislike(worldId, IPlacesAnalytics.ActionSource.FromExplore, true);
            }
            else
                placesAnalytics.RemoveVote(worldId, IPlacesAnalytics.ActionSource.FromExplore, true);

            placesAPI.SetPlaceVote(isUpvote, worldId, disposeCts.Token);
        }
    }

    private void View_OnFavoritesClicked(string worldId, bool isFavorite)
    {
        if (userProfileBridge.GetOwn().isGuest)
            dataStore.HUDs.connectWalletModalVisible.Set(true);
        else
        {
            if (isFavorite)
                placesAnalytics.AddFavorite(worldId, IPlacesAnalytics.ActionSource.FromExplore, true);
            else
                placesAnalytics.RemoveFavorite(worldId, IPlacesAnalytics.ActionSource.FromExplore, true);

            placesAPI.SetPlaceFavorite(worldId, isFavorite, disposeCts.Token);
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
        placesAnalytics.SortWorlds(view.sort == MOST_ACTIVE_FILTER_ID ? IPlacesAnalytics.SortingType.MostActive : IPlacesAnalytics.SortingType.Best);
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
            (IReadOnlyList<WorldsResponse.WorldInfo> worlds, int total) firstPage = await worldsAPIService.GetWorlds(0, PAGE_SIZE, "", view.sort, ct);
            friendsTrackerController.RemoveAllHandlers();
            worldsFromAPI.Clear();
            worldsFromAPI.AddRange(firstPage.worlds);
            if (firstPage.total > PAGE_SIZE)
            {
                (IReadOnlyList<WorldsResponse.WorldInfo> worlds, int total) secondPage = await worldsAPIService.GetWorlds(1, PAGE_SIZE, "", view.sort, ct);
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
        (IReadOnlyList<WorldsResponse.WorldInfo> worlds, int total) = await worldsAPIService.GetWorlds((worldsFromAPI.Count/PAGE_SIZE), PAGE_SIZE, "", view.sort, showMoreCts.Token);

        worldsFromAPI.AddRange(worlds);
        view.AddWorlds(PlacesAndEventsCardsFactory.ConvertWorldsResponseToModel(worlds));
        view.SetShowMoreWorldsButtonActive(worldsFromAPI.Count < total);
    }

    internal void ShowWorldDetailedInfo(PlaceCardComponentModel worldModel)
    {
        view.ShowWorldModal(worldModel);
        exploreV2Analytics.SendClickOnWorldInfo(worldModel.placeInfo.id, worldModel.placeName);
        dataStore.exploreV2.currentVisibleModal.Set(ExploreV2CurrentModal.Places);
    }

    internal void OnJumpInToWorld(PlaceInfo worldFromAPI)
    {
        JumpInToWorld(worldFromAPI);
        view.HideWorldModal();

        dataStore.exploreV2.currentVisibleModal.Set(ExploreV2CurrentModal.None);
        OnCloseExploreV2?.Invoke();
        exploreV2Analytics.SendWorldTeleport(worldFromAPI.id, worldFromAPI.title);
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

    public static void JumpInToWorld(PlaceInfo worldFromAPI)
    {
        Environment.i.world.teleportController.JumpInWorld(worldFromAPI.world_name);
    }
}
