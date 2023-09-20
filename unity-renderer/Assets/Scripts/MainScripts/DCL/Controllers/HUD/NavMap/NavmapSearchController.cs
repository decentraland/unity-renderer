using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCLServices.PlacesAPIService;
using MainScripts.DCL.Controllers.HotScenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DCL.Tasks;
using UnityEngine;

public class NavmapSearchController : IDisposable
{
    private const string PREVIOUS_SEARCHES_KEY = "previous_searches";

    private readonly IPlacesAPIService placesAPIService;
    private readonly INavmapSearchComponentView view;
    private readonly IPlayerPrefs playerPrefs;

    private CancellationTokenSource searchCts;

    public NavmapSearchController(INavmapSearchComponentView view, IPlacesAPIService placesAPIService, IPlayerPrefs playerPrefs)
    {
        this.view = view;
        this.placesAPIService = placesAPIService;
        this.playerPrefs = playerPrefs;

        searchCts = new CancellationTokenSource();
        view.OnSelectedSearchBar += OnSelectedSearchbarChange;
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

        searchCts = searchCts.SafeRestart();
        SearchAndDisplay(searchText, searchCts).Forget();
    }

    private async UniTaskVoid SearchAndDisplay(string searchText, CancellationTokenSource cts)
    {
        (IReadOnlyList<IHotScenesController.PlaceInfo> places, int total) searchPlaces = await placesAPIService.SearchPlaces(searchText, 0, 5, cts.Token);
        view.SetSearchResultRecords(searchPlaces.places);
    }

    private void OnSelectedSearchbarChange(bool isSelected)
    {
        if (isSelected)
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
        searchCts = searchCts.SafeRestart();
        string[] previousSearches = GetPreviousSearches();

        if (previousSearches.Length > 0)
            view.SetHistoryRecords(previousSearches);
    }

    public void Dispose()
    {
    }

    private void AddToPreviousSearch(string searchToAdd)
    {
        string playerPrefsPreviousSearches = playerPrefs.GetString(PREVIOUS_SEARCHES_KEY);
        string[] previousSearches = string.IsNullOrEmpty(playerPrefsPreviousSearches) ? Array.Empty<string>() : playerPrefsPreviousSearches.Split('|');
        if (previousSearches.Length < 5)
        {
            playerPrefs.Set(PREVIOUS_SEARCHES_KEY, previousSearches.Length > 0 ? searchToAdd + "|" + string.Join("|", previousSearches) : searchToAdd);
        }
        else
        {
            playerPrefs.Set(PREVIOUS_SEARCHES_KEY, searchToAdd + "|" + string.Join("|", previousSearches.Take(4)));
        }
    }

    private string[] GetPreviousSearches()
    {
        string previousSearches = playerPrefs.GetString(PREVIOUS_SEARCHES_KEY, "");
        return string.IsNullOrEmpty(previousSearches) ? Array.Empty<string>() : previousSearches.Split('|');
    }
}
