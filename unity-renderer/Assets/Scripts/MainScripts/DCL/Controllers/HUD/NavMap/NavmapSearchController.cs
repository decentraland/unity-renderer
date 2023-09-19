using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCLServices.PlacesAPIService;
using MainScripts.DCL.Controllers.HotScenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class NavmapSearchController : IDisposable
{
    private const string PREVIOUS_SEARCHES_KEY = "previous_searches";

    private readonly IPlacesAPIService placesAPIService;
    private readonly INavmapSearchComponentView view;
    private readonly IPlayerPrefs playerPrefs;

    public NavmapSearchController(INavmapSearchComponentView view, IPlacesAPIService placesAPIService, IPlayerPrefs playerPrefs)
    {
        this.view = view;
        this.placesAPIService = placesAPIService;
        this.playerPrefs = playerPrefs;

        view.OnFocusedSearchBar += OnFocusChange;
        view.OnSearchedText += OnSearchedText;
    }

    private void OnSearchedText(string searchText)
    {
        if (string.IsNullOrEmpty(searchText))
        {
            GetAndShowPreviousSearches();
            return;
        }

        AddToPreviousSearch(searchText);
        SearchAndDisplay(searchText).Forget();
    }

    private async UniTaskVoid SearchAndDisplay(string searchText)
    {
        (IReadOnlyList<IHotScenesController.PlaceInfo> places, int total) searchPlaces = await placesAPIService.SearchPlaces(searchText, 0, 5, CancellationToken.None);
        view.SetSearchResultRecords(searchPlaces.places);
    }

    private void OnFocusChange(bool isFocused)
    {
        if (isFocused)
        {
            GetAndShowPreviousSearches();
        }
        else
        {
            view.ClearResults();
        }
    }

    private void GetAndShowPreviousSearches()
    {
            string[] previousSearches = GetPreviousSearches();

            if (previousSearches.Length > 0)
                view.SetHistoryRecords(previousSearches);
    }

    public void Dispose()
    {
    }

    private void AddToPreviousSearch(string searchToAdd)
    {
        string[] previousSearches = playerPrefs.GetString(PREVIOUS_SEARCHES_KEY, "").Split('|');
    }

    private string[] GetPreviousSearches() =>
        playerPrefs.GetString(PREVIOUS_SEARCHES_KEY, "").Split('|');
}
