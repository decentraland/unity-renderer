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
using System.Linq;
using System.Threading;
using UnityEngine;

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

    private CancellationTokenSource minimalSearchCts;
    private CancellationTokenSource fullSearchCts;

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
        view.OnVoteChanged += ChangeVote;
        view.OnPlaceJumpInClicked += JumpInToPlace;
        view.OnBackFromSearch += CloseSearchPanel;
        view.OnPlaceFavoriteChanged += ChangePlaceFavorite;
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
                    placesAnalytics.Like(placeId, IPlacesAnalytics.ActionSource.FromSearch);
                else
                    placesAnalytics.Dislike(placeId, IPlacesAnalytics.ActionSource.FromSearch);
            }
            else
                placesAnalytics.RemoveVote(placeId, IPlacesAnalytics.ActionSource.FromSearch);

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
                placesAnalytics.AddFavorite(placeId, IPlacesAnalytics.ActionSource.FromSearch);
            else
                placesAnalytics.RemoveFavorite(placeId, IPlacesAnalytics.ActionSource.FromSearch);

            placesAPIService.SetPlaceFavorite(placeId, isFavorite, default).Forget();
        }
    }

    private void CloseSearchPanel()
    {
    }

    private void JumpInToPlace(IHotScenesController.PlaceInfo place)
    {
        OnCloseExploreV2?.Invoke();
        PlacesSubSectionComponentController.JumpInToPlace(place);
        exploreV2Analytics.SendPlaceTeleport(place.id, place.title, Utils.ConvertStringToVector(place.base_position), ActionSource.FromSearch);
    }

    private void OpenPlaceDetailsModal(PlaceCardComponentModel placeModel, int index)
    {
        view.ShowPlaceModal(placeModel);
        exploreV2Analytics.SendClickOnPlaceInfo(placeModel.placeInfo.id, placeModel.placeName, index, ActionSource.FromSearch);
    }

    private void Search(string searchText)
    {
        minimalSearchCts.SafeCancelAndDispose();
        minimalSearchCts = new CancellationTokenSource();

        view.SetAllAsLoading();
        view.SetHeaderEnabled(searchText);
        RequestFavoritePlaces(cancellationToken: minimalSearchCts.Token).Forget();
        RequestFavoriteWorlds(cancellationToken: minimalSearchCts.Token).Forget();
    }

    private void RequestAllFavoritePlaces(int pageNumber)
    {
        fullSearchCts.SafeCancelAndDispose();
        fullSearchCts = new CancellationTokenSource();
        RequestFavoritePlaces(pageNumber, 18, fullSearchCts.Token, true).Forget();
    }

    private void RequestAllFavoriteWorlds(int pageNumber)
    {
        fullSearchCts.SafeCancelAndDispose();
        fullSearchCts = new CancellationTokenSource();
        RequestFavoriteWorlds(pageNumber, 18, fullSearchCts.Token, true).Forget();
    }

    private async UniTaskVoid RequestFavoritePlaces(int pageNumber = 0, int pageSize = 6, CancellationToken cancellationToken = default, bool isFullSearch = false)
    {
        var results = await placesAPIService.GetFavorites(pageNumber, pageSize, cancellationToken);
        List<PlaceCardComponentModel> places = PlacesAndEventsCardsFactory.ConvertPlaceResponseToModel(results);
        //exploreV2Analytics.SendSearchPlaces(searchText, places.Select(p=>p.coords).ToArray(), places.Select(p=>p.placeInfo.id).ToArray());

        if (isFullSearch)
        {
            view.ShowAllPlaces(places, (pageNumber + 1) * pageSize < results.Count);
        }
        else
        {
            view.ShowPlaces(places);
        }
    }

    private async UniTaskVoid RequestFavoriteWorlds(int pageNumber = 0, int pageSize = 6, CancellationToken cancellationToken = default, bool isFullSearch = false)
    {
        /*
        var results = await worldsAPIService.SearchWorlds(pageNumber, pageSize, cancellationToken);
        List<PlaceCardComponentModel> worlds = PlacesAndEventsCardsFactory.ConvertWorldsResponseToModel(results.Item1);
        exploreV2Analytics.SendSearchWorlds(searchText, worlds.Select(p=>p.placeInfo.id).ToArray());

        if (isFullSearch)
        {
            view.ShowAllWorlds(worlds, (pageNumber + 1) * pageSize < results.total);
        }
        else
        {
            view.ShowWorlds(worlds);
        }
        */
    }

    public void Dispose()
    {
        view.OnRequestAllPlaces -= RequestAllFavoritePlaces;
        view.OnRequestAllWorlds -= RequestAllFavoriteWorlds;
        view.OnPlaceInfoClicked -= OpenPlaceDetailsModal;
        view.OnPlaceJumpInClicked -= JumpInToPlace;
        view.OnBackFromSearch -= CloseSearchPanel;
        view.OnPlaceFavoriteChanged -= ChangePlaceFavorite;
    }
}
