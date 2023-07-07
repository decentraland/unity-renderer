using Cysharp.Threading.Tasks;
using DCL;
using DCL.Tasks;
using DCLServices.PlacesAPIService;
using MainScripts.DCL.Helpers.Utils;
using System;
using System.Collections.Generic;
using DCL.Helpers;
using System.Threading;
using UnityEngine;

public class SearchSubSectionComponentController : ISearchSubSectionComponentController
{
    private ISearchSubSectionComponentView view;
    private SearchBarComponentView searchBarComponentView;
    private IEventsAPIController eventsAPI;
    private IUserProfileBridge userProfileBridge;
    private DataStore dataStore;

    private CancellationTokenSource minimalSearchCts;
    private CancellationTokenSource fullSearchCts;

    public SearchSubSectionComponentController(ISearchSubSectionComponentView view,
        SearchBarComponentView searchBarComponentView,
        IEventsAPIController eventsAPI,
        IUserProfileBridge userProfileBridge,
        DataStore dataStore)
    {
        this.view = view;
        this.searchBarComponentView = searchBarComponentView;
        this.eventsAPI = eventsAPI;
        this.userProfileBridge = userProfileBridge;
        this.dataStore = dataStore;

        view.OnRequestAllEvents += SearchAllEvents;

        view.OnInfoClicked += OpenEventDetailsModal;
        view.OnSubscribeEventClicked += SubscribeToEvent;
        view.OnUnsubscribeEventClicked += UnsubscribeToEvent;
        view.OnJumpInClicked += JumpInToEvent;

        if(searchBarComponentView != null)
            searchBarComponentView.OnSearchText += Search;
    }

    private void JumpInToEvent(EventFromAPIModel obj)
    {
        throw new NotImplementedException();
    }

    private void OpenEventDetailsModal(EventCardComponentModel eventModel)
    {
        view.ShowEventModal(eventModel);
        //exploreV2Analytics.SendClickOnEventInfo(eventModel.eventId, eventModel.eventName);
    }

    private void Search(string searchText)
    {
        minimalSearchCts.SafeCancelAndDispose();
        minimalSearchCts = new CancellationTokenSource();

        view.SetAllAsLoading();
        SearchEvents(searchText, cancellationToken: minimalSearchCts.Token).Forget();
        //SearchPlaces(searchText, cts.Token).Forget();
    }

    private void SubscribeToEvent(string eventId)
    {
        if (userProfileBridge.GetOwn().isGuest)
            dataStore.HUDs.connectWalletModalVisible.Set(true);
        else
            eventsAPI.RegisterParticipation(eventId);
    }

    private void UnsubscribeToEvent(string eventId) =>
        eventsAPI.RemoveParticipation(eventId);

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

    private async UniTaskVoid SearchPlaces(string searchText, CancellationToken cancellationToken)
    {
        var results = await eventsAPI.SearchEvents(searchText, 0,5, cancellationToken);
        List<EventCardComponentModel> trendingEvents = PlacesAndEventsCardsFactory.CreateEventsCards(results.Item1);
        view.ShowEvents(trendingEvents, searchText);
    }

    public void Dispose()
    {
    }
}
