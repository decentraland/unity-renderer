using Cysharp.Threading.Tasks;
using DCL.Tasks;
using DCLServices.PlacesAPIService;
using MainScripts.DCL.Helpers.Utils;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class SearchSubSectionComponentController : ISearchSubSectionComponentController
{
    private ISearchSubSectionComponentView view;
    private SearchBarComponentView searchBarComponentView;
    private IEventsAPIController eventsAPI;

    private CancellationTokenSource minimalSearchCts;
    private CancellationTokenSource fullSearchCts;

    public SearchSubSectionComponentController(ISearchSubSectionComponentView view,
        SearchBarComponentView searchBarComponentView,
        IEventsAPIController eventsAPI)
    {
        this.view = view;
        this.searchBarComponentView = searchBarComponentView;
        this.eventsAPI = eventsAPI;

        view.OnRequestAllEvents += SearchAllEvents;
        searchBarComponentView.OnSearchText += Search;
    }

    private void Search(string searchText)
    {
        minimalSearchCts.SafeCancelAndDispose();
        minimalSearchCts = new CancellationTokenSource();
        SearchEvents(searchText, cancellationToken: minimalSearchCts.Token).Forget();
        //SearchPlaces(searchText, cts.Token).Forget();
    }

    private void SearchAllEvents()
    {
        fullSearchCts.SafeCancelAndDispose();
        fullSearchCts = new CancellationTokenSource();
        SearchEvents(searchBarComponentView.Text, 0, 18, fullSearchCts.Token, true).Forget();
    }

    private async UniTaskVoid SearchEvents(string searchText, int pageNumber = 0, int pageSize = 6, CancellationToken cancellationToken = default, bool isFullSearch = false)
    {
        var results = await eventsAPI.SearchEvents(searchText, pageNumber,pageSize, cancellationToken);
        List<EventCardComponentModel> trendingEvents = PlacesAndEventsCardsFactory.CreateEventsCards(results);

        if (isFullSearch)
        {
            view.ShowAllEvents(trendingEvents);
        }
        else
        {
            view.ShowEvents(trendingEvents);
        }
    }

    private async UniTaskVoid SearchPlaces(string searchText, CancellationToken cancellationToken)
    {
        var results = await eventsAPI.SearchEvents(searchText, 0,5, cancellationToken);
        List<EventCardComponentModel> trendingEvents = PlacesAndEventsCardsFactory.CreateEventsCards(results);
        view.ShowEvents(trendingEvents);
    }

    public void Dispose()
    {
    }
}
