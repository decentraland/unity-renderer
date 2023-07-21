using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using DCL.Tasks;
using DCLServices.PlacesAPIService;
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
    private readonly IUserProfileBridge userProfileBridge;
    private readonly IExploreV2Analytics exploreV2Analytics;
    private readonly IPlacesAnalytics placesAnalytics;
    private readonly DataStore dataStore;

    public event Action OnCloseExploreV2;

    private CancellationTokenSource minimalSearchCts;
    private CancellationTokenSource fullSearchCts;

    public SearchSubSectionComponentController(ISearchSubSectionComponentView view,
        SearchBarComponentView searchBarComponentView,
        IEventsAPIController eventsAPI,
        IPlacesAPIService placesAPIService,
        IUserProfileBridge userProfileBridge,
        IExploreV2Analytics exploreV2Analytics,
        IPlacesAnalytics placesAnalytics,
        DataStore dataStore)
    {
        this.view = view;
        this.searchBarComponentView = searchBarComponentView;
        this.eventsAPI = eventsAPI;
        this.placesAPIService = placesAPIService;
        this.userProfileBridge = userProfileBridge;
        this.exploreV2Analytics = exploreV2Analytics;
        this.placesAnalytics = placesAnalytics;
        this.dataStore = dataStore;

        view.OnRequestAllEvents += SearchAllEvents;
        view.OnRequestAllPlaces += SearchAllPlaces;
        view.OnEventInfoClicked += OpenEventDetailsModal;
        view.OnPlaceInfoClicked += OpenPlaceDetailsModal;
        view.OnSubscribeEventClicked += SubscribeToEvent;
        view.OnUnsubscribeEventClicked += UnsubscribeToEvent;
        view.OnEventJumpInClicked += JumpInToEvent;
        view.OnPlaceJumpInClicked += JumpInToPlace;
        view.OnBackFromSearch += CloseSearchPanel;
        view.OnPlaceFavoriteChanged += ChangePlaceFavorite;

        if(searchBarComponentView != null)
            searchBarComponentView.OnSearchText += Search;
    }

    private void ChangePlaceFavorite(string placeId, bool isFavorite)
    {
        if(isFavorite)
            placesAnalytics.AddFavorite(placeId, IPlacesAnalytics.ActionSource.FromSearch);
        else
            placesAnalytics.RemoveFavorite(placeId, IPlacesAnalytics.ActionSource.FromSearch);

        placesAPIService.SetPlaceFavorite(placeId, isFavorite, default).Forget();
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

    private async UniTaskVoid SearchEvents(string searchText, int pageNumber = 0, int pageSize = 6, CancellationToken cancellationToken = default, bool isFullSearch = false)
    {
        var results = await eventsAPI.SearchEvents(searchText, pageNumber,pageSize, cancellationToken);
        List<EventCardComponentModel> searchedEvents = PlacesAndEventsCardsFactory.CreateEventsCards(results.Item1);
        exploreV2Analytics.SendSearchEvents(searchText, searchedEvents.Select(e=>e.coords).ToArray(), searchedEvents.Select(p=>p.eventFromAPIInfo.id).ToArray());

        if (isFullSearch)
        {
            view.ShowAllEvents(searchedEvents, (pageNumber + 1) * pageSize < results.total);
        }
        else
        {
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

    public void Dispose()
    {
        view.OnRequestAllEvents -= SearchAllEvents;
        view.OnRequestAllPlaces -= SearchAllPlaces;
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
