using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using DCL.Tasks;
using DCLServices.PlacesAPIService;
using DCLServices.WorldsAPIService;
using System.Collections.Generic;
using ExploreV2Analytics;
using MainScripts.DCL.Controllers.HotScenes;
using System;
using System.Threading;
using Environment = DCL.Environment;

public class FavoriteSubSectionComponentController : IFavoriteSubSectionComponentController
{
    private readonly IFavoriteSubSectionComponentView view;
    private readonly IPlacesAPIService placesAPIService;
    private readonly IWorldsAPIService worldsAPIService;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly IExploreV2Analytics exploreV2Analytics;
    private readonly IPlacesAnalytics placesAnalytics;
    private readonly DataStore dataStore;

    public event Action OnCloseExploreV2;

    private CancellationTokenSource minimalListCts;
    private CancellationTokenSource fullFavoriteListCts;

    public FavoriteSubSectionComponentController(
        IFavoriteSubSectionComponentView view,
        IPlacesAPIService placesAPIService,
        IWorldsAPIService worldsAPIService,
        IUserProfileBridge userProfileBridge,
        IExploreV2Analytics exploreV2Analytics,
        IPlacesAnalytics placesAnalytics,
        DataStore dataStore)
    {
        this.view = view;
        this.placesAPIService = placesAPIService;
        this.worldsAPIService = worldsAPIService;
        this.userProfileBridge = userProfileBridge;
        this.exploreV2Analytics = exploreV2Analytics;
        this.placesAnalytics = placesAnalytics;
        this.dataStore = dataStore;

        view.OnRequestAllPlaces += RequestAllFavoritePlaces;
        view.OnRequestAllWorlds += RequestAllFavoriteWorlds;
        view.OnPlaceInfoClicked += OpenPlaceDetailsModal;
        view.OnWorldInfoClicked += OpenWorldDetailsModal;
        view.OnVoteChanged += ChangeVote;
        view.OnPlaceJumpInClicked += JumpInToPlace;
        view.OnWorldJumpInClicked += JumpInToWorld;
        view.OnPlaceFavoriteChanged += ChangePlaceFavorite;
        view.OnRequestFavorites += RequestFavorites;
    }

    private void ChangeVote(string placeId, bool? isUpvote)
    {
        if (userProfileBridge.GetOwn().isGuest)
            dataStore.HUDs.connectWalletModalVisible.Set(true);
        else
        {
            if (isUpvote != null)
            {
                if (isUpvote.Value)
                    placesAnalytics.Like(placeId, IPlacesAnalytics.ActionSource.FromFavorites);
                else
                    placesAnalytics.Dislike(placeId, IPlacesAnalytics.ActionSource.FromFavorites);
            }
            else
                placesAnalytics.RemoveVote(placeId, IPlacesAnalytics.ActionSource.FromFavorites);

            placesAPIService.SetPlaceVote(isUpvote, placeId, default).Forget();
        }
    }

    private void ChangePlaceFavorite(string placeId, bool isFavorite)
    {
        if (userProfileBridge.GetOwn().isGuest)
            dataStore.HUDs.connectWalletModalVisible.Set(true);
        else
        {
            if(isFavorite)
                placesAnalytics.AddFavorite(placeId, IPlacesAnalytics.ActionSource.FromFavorites);
            else
                placesAnalytics.RemoveFavorite(placeId, IPlacesAnalytics.ActionSource.FromFavorites);

            placesAPIService.SetPlaceFavorite(placeId, isFavorite, default).Forget();
        }
    }

    private void JumpInToPlace(IHotScenesController.PlaceInfo place)
    {
        OnCloseExploreV2?.Invoke();
        PlacesSubSectionComponentController.JumpInToPlace(place);
        exploreV2Analytics.SendPlaceTeleport(place.id, place.title, Utils.ConvertStringToVector(place.base_position), ActionSource.FromFavorites);
    }

    private void OpenPlaceDetailsModal(PlaceCardComponentModel placeModel, int index)
    {
        view.ShowPlaceModal(placeModel);
        exploreV2Analytics.SendClickOnPlaceInfo(placeModel.placeInfo.id, placeModel.placeName, index, ActionSource.FromFavorites);
    }

    private void OpenWorldDetailsModal(PlaceCardComponentModel placeModel, int index)
    {
        view.ShowWorldModal(placeModel);
        exploreV2Analytics.SendClickOnWorldInfo(placeModel.placeInfo.id, placeModel.placeName, index, ActionSource.FromFavorites);
    }

    private void RequestFavorites()
    {
        view.SetHeaderEnabled(false);
        minimalListCts = minimalListCts.SafeRestart();

        view.SetAllAsLoading();
        RequestFavoritePlaces(cancellationToken: minimalListCts.Token).Forget();
        RequestFavoriteWorlds(cancellationToken: minimalListCts.Token).Forget();
    }

    private void RequestAllFavoritePlaces(int pageNumber)
    {
        view.SetHeaderEnabled(true);
        fullFavoriteListCts = fullFavoriteListCts.SafeRestart();
        RequestFavoritePlaces(pageNumber, 18, fullFavoriteListCts.Token, true).Forget();
    }

    private void RequestAllFavoriteWorlds(int pageNumber)
    {
        view.SetHeaderEnabled(true);
        fullFavoriteListCts = fullFavoriteListCts.SafeRestart();
        RequestFavoriteWorlds(pageNumber, 18, fullFavoriteListCts.Token, true).Forget();
    }

    private async UniTaskVoid RequestFavoritePlaces(int pageNumber = 0, int pageSize = 6, CancellationToken cancellationToken = default, bool isFullList = false)
    {
        var results = await placesAPIService.GetFavorites(pageNumber, pageSize, cancellationToken, true);
        List<PlaceCardComponentModel> places = PlacesAndEventsCardsFactory.ConvertPlaceResponseToModel(results);

        if (isFullList)
        {
            view.ShowAllPlaces(places, (pageNumber + 1) * pageSize < results.Count);
        }
        else
        {
            view.ShowPlaces(places);
        }
    }

    private async UniTaskVoid RequestFavoriteWorlds(int pageNumber = 0, int pageSize = 6, CancellationToken cancellationToken = default, bool isFullList = false)
    {
        var results = await worldsAPIService.GetFavorites(pageNumber, pageSize, cancellationToken);
        List<PlaceCardComponentModel> worlds = PlacesAndEventsCardsFactory.ConvertWorldsResponseToModel(results);

        if (isFullList)
        {
            view.ShowAllWorlds(worlds, false);
        }
        else
        {
            view.ShowWorlds(worlds);
        }
    }

    internal void JumpInToWorld(IHotScenesController.PlaceInfo worldFromAPI)
    {
        Environment.i.world.teleportController.JumpInWorld(worldFromAPI.world_name);
        view.HideWorldModal();
        dataStore.exploreV2.currentVisibleModal.Set(ExploreV2CurrentModal.None);
        OnCloseExploreV2?.Invoke();
        exploreV2Analytics.SendWorldTeleport(worldFromAPI.id, worldFromAPI.title);
    }

    public void Dispose()
    {
        minimalListCts.SafeCancelAndDispose();
        fullFavoriteListCts.SafeCancelAndDispose();
        view.OnRequestAllPlaces -= RequestAllFavoritePlaces;
        view.OnRequestAllWorlds -= RequestAllFavoriteWorlds;
        view.OnPlaceInfoClicked -= OpenPlaceDetailsModal;
        view.OnWorldInfoClicked -= OpenWorldDetailsModal;
        view.OnVoteChanged -= ChangeVote;
        view.OnPlaceJumpInClicked -= JumpInToPlace;
        view.OnWorldJumpInClicked -= JumpInToWorld;
        view.OnPlaceFavoriteChanged -= ChangePlaceFavorite;
        view.OnRequestFavorites -= RequestFavorites;
    }
}
