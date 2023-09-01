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

public class SearchSubSectionComponentController : ISearchSubSectionComponentController
{
    private readonly ISearchSubSectionComponentView view;
    private readonly SearchBarComponentView searchBarComponentView;
    private readonly IEventsAPIController eventsAPI;
    private readonly IPlacesAPIService placesAPIService;
    private readonly IWorldsAPIService worldsAPIService;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly IExploreV2Analytics exploreV2Analytics;
    private readonly IPlacesAnalytics placesAnalytics;
    private readonly DataStore dataStore;

    public event Action OnCloseExploreV2;

    private CancellationTokenSource minimalSearchCts;
    private CancellationTokenSource fullSearchCts;
    private CancellationTokenSource getPlacesAssociatedToEventsCts;

    public SearchSubSectionComponentController(ISearchSubSectionComponentView view,
        SearchBarComponentView searchBarComponentView,
        IEventsAPIController eventsAPI,
        IPlacesAPIService placesAPIService,
        IWorldsAPIService worldsAPIService,
        IUserProfileBridge userProfileBridge,
        IExploreV2Analytics exploreV2Analytics,
        IPlacesAnalytics placesAnalytics,
        DataStore dataStore)
    {
        this.view = view;
        this.searchBarComponentView = searchBarComponentView;
        this.eventsAPI = eventsAPI;
        this.placesAPIService = placesAPIService;
        this.worldsAPIService = worldsAPIService;
        this.userProfileBridge = userProfileBridge;
        this.exploreV2Analytics = exploreV2Analytics;
        this.placesAnalytics = placesAnalytics;
        this.dataStore = dataStore;

        view.OnRequestAllEvents += SearchAllEvents;
        view.OnRequestAllPlaces += SearchAllPlaces;
        view.OnRequestAllWorlds += SearchAllWorlds;
        view.OnEventInfoClicked += OpenEventDetailsModal;
        view.OnPlaceInfoClicked += OpenPlaceDetailsModal;
        view.OnVoteChanged += ChangeVote;
        view.OnSubscribeEventClicked += SubscribeToEvent;
        view.OnUnsubscribeEventClicked += UnsubscribeToEvent;
        view.OnEventJumpInClicked += JumpInToEvent;
        view.OnPlaceJumpInClicked += JumpInToPlace;
        view.OnBackFromSearch += CloseSearchPanel;
        view.OnPlaceFavoriteChanged += ChangePlaceFavorite;

        if(searchBarComponentView != null)
            searchBarComponentView.OnSearchText += Search;
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
        searchBarComponentView.SubmitSearch("");
    }

    private void JumpInToEvent(EventFromAPIModel eventFromAPI)
    {
        OnCloseExploreV2?.Invoke();
        EventsSubSectionComponentController.JumpInToEvent(eventFromAPI);
        exploreV2Analytics.SendEventTeleport(eventFromAPI.id, eventFromAPI.name, new Vector2Int(eventFromAPI.coordinates[0], eventFromAPI.coordinates[1]), ActionSource.FromSearch);
    }

    private void JumpInToPlace(IHotScenesController.PlaceInfo place)
    {
        OnCloseExploreV2?.Invoke();
        PlacesSubSectionComponentController.JumpInToPlace(place);
        exploreV2Analytics.SendPlaceTeleport(place.id, place.title, Utils.ConvertStringToVector(place.base_position), ActionSource.FromSearch);
    }

    private void OpenEventDetailsModal(EventCardComponentModel eventModel, int index)
    {
        view.ShowEventModal(eventModel);
        exploreV2Analytics.SendClickOnEventInfo(eventModel.eventId, eventModel.eventName, index, ActionSource.FromSearch);
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
        SearchEvents(searchText, cancellationToken: minimalSearchCts.Token).Forget();
        SearchPlaces(searchText, cancellationToken: minimalSearchCts.Token).Forget();
        SearchWorlds(searchText, cancellationToken: minimalSearchCts.Token).Forget();
    }

    private void SubscribeToEvent(string eventId)
    {
        if (userProfileBridge.GetOwn().isGuest)
            dataStore.HUDs.connectWalletModalVisible.Set(true);
        else
        {
            exploreV2Analytics.SendParticipateEvent(eventId, ActionSource.FromSearch);
            eventsAPI.RegisterParticipation(eventId);
        }
    }

    private void UnsubscribeToEvent(string eventId)
    {
        eventsAPI.RemoveParticipation(eventId);
        exploreV2Analytics.SendParticipateEvent(eventId, ActionSource.FromSearch);
    }

    private void SearchAllEvents(int pageNumber)
    {
        fullSearchCts.SafeCancelAndDispose();
        fullSearchCts = new CancellationTokenSource();
        SearchEvents(searchBarComponentView.Text, pageNumber, 18, fullSearchCts.Token, true).Forget();
    }

    private void SearchAllPlaces(int pageNumber)
    {
        fullSearchCts.SafeCancelAndDispose();
        fullSearchCts = new CancellationTokenSource();
        SearchPlaces(searchBarComponentView.Text, pageNumber, 18, fullSearchCts.Token, true).Forget();
    }

    private void SearchAllWorlds(int pageNumber)
    {
        fullSearchCts.SafeCancelAndDispose();
        fullSearchCts = new CancellationTokenSource();
        SearchWorlds(searchBarComponentView.Text, pageNumber, 18, fullSearchCts.Token, true).Forget();
    }

    private async UniTaskVoid SearchEvents(string searchText, int pageNumber = 0, int pageSize = 6, CancellationToken cancellationToken = default, bool isFullSearch = false)
    {
        var results = await eventsAPI.SearchEvents(searchText, pageNumber,pageSize, cancellationToken);

        getPlacesAssociatedToEventsCts = getPlacesAssociatedToEventsCts.SafeRestart();
        GetPlacesAssociatedToEventsAsync(results, getPlacesAssociatedToEventsCts.Token).Forget();

        async UniTaskVoid GetPlacesAssociatedToEventsAsync((List<EventFromAPIModel> eventsFromAPI, int total) searchData, CancellationToken ct)
        {
            // Land's events
            var landEventsFromAPI = searchData.eventsFromAPI.Where(e => !e.world).ToList();
            var coordsList = landEventsFromAPI.Select(e => new Vector2Int(e.coordinates[0], e.coordinates[1]));
            var places = await placesAPIService.GetPlacesByCoordsList(coordsList, ct);

            foreach (EventFromAPIModel landEventFromAPI in landEventsFromAPI)
            {
                Vector2Int landEventCoords = new Vector2Int(landEventFromAPI.coordinates[0], landEventFromAPI.coordinates[1]);
                foreach (IHotScenesController.PlaceInfo place in places)
                {
                    if (!place.Positions.Contains(landEventCoords))
                        continue;

                    landEventFromAPI.scene_name = place.title;
                    break;
                }
            }

            // World's events
            var worldEventsFromAPI = searchData.eventsFromAPI.Where(e => e.world).ToList();
            var worldNamesList = worldEventsFromAPI.Select(e => e.server);
            var worlds = await worldsAPIService.GetWorldsByNamesList(worldNamesList, ct);

            foreach (EventFromAPIModel worldEventFromAPI in worldEventsFromAPI)
            {
                foreach (WorldsResponse.WorldInfo world in worlds)
                {
                    if (world.world_name != worldEventFromAPI.server)
                        continue;

                    worldEventFromAPI.scene_name = world.title;
                    break;
                }
            }

            List<EventCardComponentModel> searchedEvents = PlacesAndEventsCardsFactory.CreateEventsCards(searchData.eventsFromAPI);
            exploreV2Analytics.SendSearchEvents(searchText, searchedEvents.Select(e=>e.coords).ToArray(), searchedEvents.Select(p=>p.eventFromAPIInfo.id).ToArray());

            if (isFullSearch)
                view.ShowAllEvents(searchedEvents, (pageNumber + 1) * pageSize < searchData.total);
            else
                view.ShowEvents(searchedEvents, searchText);
        }
    }

    private async UniTaskVoid SearchPlaces(string searchText, int pageNumber = 0, int pageSize = 6, CancellationToken cancellationToken = default, bool isFullSearch = false)
    {
        var results = await placesAPIService.SearchPlaces(searchText, pageNumber, pageSize, cancellationToken);
        List<PlaceCardComponentModel> places = PlacesAndEventsCardsFactory.ConvertPlaceResponseToModel(results.Item1);
        exploreV2Analytics.SendSearchPlaces(searchText, places.Select(p=>p.coords).ToArray(), places.Select(p=>p.placeInfo.id).ToArray());

        if (isFullSearch)
        {
            view.ShowAllPlaces(places, (pageNumber + 1) * pageSize < results.total);
        }
        else
        {
            view.ShowPlaces(places, searchText);
        }
    }

    private async UniTaskVoid SearchWorlds(string searchText, int pageNumber = 0, int pageSize = 6, CancellationToken cancellationToken = default, bool isFullSearch = false)
    {
        var results = await worldsAPIService.SearchWorlds(searchText, pageNumber, pageSize, cancellationToken);
        List<PlaceCardComponentModel> worlds = PlacesAndEventsCardsFactory.ConvertWorldsResponseToModel(results.Item1);
        exploreV2Analytics.SendSearchWorlds(searchText, worlds.Select(p=>p.placeInfo.id).ToArray());

        if (isFullSearch)
        {
            view.ShowAllWorlds(worlds, (pageNumber + 1) * pageSize < results.total);
        }
        else
        {
            view.ShowWorlds(worlds, searchText);
        }
    }

    public void Dispose()
    {
        getPlacesAssociatedToEventsCts.SafeCancelAndDispose();

        view.OnRequestAllEvents -= SearchAllEvents;
        view.OnRequestAllPlaces -= SearchAllPlaces;
        view.OnRequestAllWorlds -= SearchAllWorlds;
        view.OnEventInfoClicked -= OpenEventDetailsModal;
        view.OnPlaceInfoClicked -= OpenPlaceDetailsModal;
        view.OnEventInfoClicked -= OpenEventDetailsModal;
        view.OnSubscribeEventClicked -= SubscribeToEvent;
        view.OnUnsubscribeEventClicked -= UnsubscribeToEvent;
        view.OnEventJumpInClicked -= JumpInToEvent;
        view.OnPlaceJumpInClicked -= JumpInToPlace;
        view.OnBackFromSearch -= CloseSearchPanel;
        view.OnPlaceFavoriteChanged -= ChangePlaceFavorite;

        if(searchBarComponentView != null)
            searchBarComponentView.OnSearchText -= Search;
    }
}
