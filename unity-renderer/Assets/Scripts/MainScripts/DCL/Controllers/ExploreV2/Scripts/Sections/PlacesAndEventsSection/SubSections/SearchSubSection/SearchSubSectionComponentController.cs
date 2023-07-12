using Cysharp.Threading.Tasks;
using DCL;
using DCL.Tasks;
using System.Collections.Generic;
using ExploreV2Analytics;
using System.Threading;
using UnityEngine;

public class SearchSubSectionComponentController : ISearchSubSectionComponentController
{
    private readonly ISearchSubSectionComponentView view;
    private readonly SearchBarComponentView searchBarComponentView;
    private readonly IEventsAPIController eventsAPI;
    private readonly IUserProfileBridge userProfileBridge;
    private readonly IExploreV2Analytics exploreV2Analytics;
    private readonly DataStore dataStore;

    private CancellationTokenSource minimalSearchCts;
    private CancellationTokenSource fullSearchCts;

    public SearchSubSectionComponentController(ISearchSubSectionComponentView view,
        SearchBarComponentView searchBarComponentView,
        IEventsAPIController eventsAPI,
        IUserProfileBridge userProfileBridge,
        IExploreV2Analytics exploreV2Analytics,
        DataStore dataStore)
    {
        this.view = view;
        this.searchBarComponentView = searchBarComponentView;
        this.eventsAPI = eventsAPI;
        this.userProfileBridge = userProfileBridge;
        this.exploreV2Analytics = exploreV2Analytics;
        this.dataStore = dataStore;

        view.OnRequestAllEvents += SearchAllEvents;
        view.OnInfoClicked += OpenEventDetailsModal;
        view.OnSubscribeEventClicked += SubscribeToEvent;
        view.OnUnsubscribeEventClicked += UnsubscribeToEvent;
        view.OnJumpInClicked += JumpInToEvent;
        view.OnBackFromSearch += CloseSearchPanel;

        if(searchBarComponentView != null)
            searchBarComponentView.OnSearchText += Search;
    }

    private void CloseSearchPanel()
    {
        searchBarComponentView.SubmitSearch("");
    }

    private void JumpInToEvent(EventFromAPIModel eventFromAPI)
    {
        EventsSubSectionComponentController.JumpInToEvent(eventFromAPI);
        exploreV2Analytics.SendEventTeleport(eventFromAPI.id, eventFromAPI.name, new Vector2Int(eventFromAPI.coordinates[0], eventFromAPI.coordinates[1]), ActionSource.FromSearch);
    }

    private void OpenEventDetailsModal(EventCardComponentModel eventModel)
    {
        view.ShowEventModal(eventModel);
        exploreV2Analytics.SendClickOnEventInfo(eventModel.eventId, eventModel.eventName, ActionSource.FromSearch);
    }

    private void Search(string searchText)
    {
        minimalSearchCts.SafeCancelAndDispose();
        minimalSearchCts = new CancellationTokenSource();

        view.SetAllAsLoading();
        view.SetHeaderEnabled(!string.IsNullOrEmpty(searchText), searchText);
        SearchEvents(searchText, cancellationToken: minimalSearchCts.Token).Forget();
        //SearchPlaces(searchText, cts.Token).Forget();
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

    private async UniTaskVoid SearchEvents(string searchText, int pageNumber = 0, int pageSize = 6, CancellationToken cancellationToken = default, bool isFullSearch = false)
    {
        var results = await eventsAPI.SearchEvents(searchText, pageNumber,pageSize, cancellationToken);
        List<EventCardComponentModel> searchedEvents = PlacesAndEventsCardsFactory.CreateEventsCards(results.Item1);

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
        var results = await eventsAPI.SearchEvents(searchText, 0,5, cancellationToken);
        List<EventCardComponentModel> trendingEvents = PlacesAndEventsCardsFactory.CreateEventsCards(results.Item1);
        view.ShowEvents(trendingEvents, searchText);
    }

    public void Dispose()
    {
        view.OnRequestAllEvents -= SearchAllEvents;

        view.OnInfoClicked -= OpenEventDetailsModal;
        view.OnSubscribeEventClicked -= SubscribeToEvent;
        view.OnUnsubscribeEventClicked -= UnsubscribeToEvent;
        view.OnJumpInClicked -= JumpInToEvent;
        view.OnBackFromSearch -= CloseSearchPanel;

        if(searchBarComponentView != null)
            searchBarComponentView.OnSearchText -= Search;
    }
}
