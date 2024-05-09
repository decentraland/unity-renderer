using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using DCLServices.PlacesAPIService;
using MainScripts.DCL.Controllers.HotScenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DCL.Tasks;
using DCLServices.MapRendererV2.ConsumerUtils;
using DCLServices.MapRendererV2.MapCameraController;
using ExploreV2Analytics;
using UnityEngine;

public class NavmapSearchController : IDisposable
{
    private const string PREVIOUS_SEARCHES_KEY = "previous_searches";

    private const float TRANSLATION_DURATION = 1;

    private readonly IPlacesAPIService placesAPIService;
    private readonly INavmapSearchComponentView view;
    private readonly IPlayerPrefs playerPrefs;
    private readonly NavmapZoomViewController navmapZoomViewController;
    private readonly INavmapToastViewController toastViewController;
    private readonly IExploreV2Analytics exploreV2Analytics;

    private CancellationTokenSource searchCts;
    private bool isAlreadySelected = false;
    private IMapCameraController mapCamera;
    private bool active;

    public NavmapSearchController(
        INavmapSearchComponentView view,
        IPlacesAPIService placesAPIService,
        IPlayerPrefs playerPrefs,
        NavmapZoomViewController navmapZoomViewController,
        INavmapToastViewController toastViewController,
        IExploreV2Analytics exploreV2Analytics)
    {
        this.view = view;
        this.placesAPIService = placesAPIService;
        this.playerPrefs = playerPrefs;
        this.navmapZoomViewController = navmapZoomViewController;
        this.toastViewController = toastViewController;
        this.exploreV2Analytics = exploreV2Analytics;

        searchCts = new CancellationTokenSource();
        view.OnSelectedSearchBar += OnSelectedSearchbarChange;
        view.OnSearchedText += OnSearchedText;
        view.OnSelectedSearchRecord += OnSelectedSearchRecord;
    }

    private void OnSelectedSearchRecord(Vector2Int coordinates)
    {
        mapCamera.TranslateTo(
            coordinates: coordinates,
            duration: TRANSLATION_DURATION,
            onComplete: () => toastViewController.ShowPlaceToast(new MapRenderImage.ParcelClickData(){Parcel = coordinates, WorldPosition = new Vector2(Screen.width / 2f, Screen.height / 2f)}, showUntilClick: true));

        OnSelectedSearchbarChange(false);
        exploreV2Analytics.SendClickedNavmapSearchResult(coordinates);
    }

    public void Activate(IMapCameraController mapCameraController)
    {
        if (active && mapCamera == mapCameraController)
            return;

        mapCamera = mapCameraController;
        active = true;
    }

    public void Deactivate()
    {
        if (!active) return;
        active = false;
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
        List<Vector2Int> resultsCoordinates = new List<Vector2Int>();
        List<string> resultsIds = new List<string>();
        foreach (var place in searchPlaces.places)
        {
            resultsCoordinates.Add(Utils.ConvertStringToVector(place.base_position));
            resultsIds.Add(place.id);
        }
        view.SetSearchResultRecords(searchPlaces.places);
        exploreV2Analytics.SendSearchPlaces(searchText, resultsCoordinates.ToArray(), resultsIds.ToArray(), ActionSource.FromNavmap);
    }

    private void OnSelectedSearchbarChange(bool isSelected)
    {
        if (isSelected == isAlreadySelected)
            return;

        isAlreadySelected = isSelected;
        if (isSelected)
            GetAndShowPreviousSearches();
        else
            view.ClearResults();
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
        view.OnSelectedSearchBar -= OnSelectedSearchbarChange;
        view.OnSearchedText -= OnSearchedText;
        view.OnSelectedSearchRecord -= OnSelectedSearchRecord;
    }

    private void AddToPreviousSearch(string searchToAdd)
    {
        string playerPrefsPreviousSearches = playerPrefs.GetString(PREVIOUS_SEARCHES_KEY);
        string[] previousSearches = string.IsNullOrEmpty(playerPrefsPreviousSearches) ? Array.Empty<string>() : playerPrefsPreviousSearches.Split('|');
        switch (previousSearches.Length)
        {
            case > 0 when previousSearches[0] == searchToAdd:
                return;
            case < 5:
                playerPrefs.Set(PREVIOUS_SEARCHES_KEY, previousSearches.Length > 0 ? searchToAdd + "|" + string.Join("|", previousSearches) : searchToAdd);
                break;
            default:
                playerPrefs.Set(PREVIOUS_SEARCHES_KEY, searchToAdd + "|" + string.Join("|", previousSearches.Take(4)));
                break;
        }
    }

    private string[] GetPreviousSearches()
    {
        string previousSearches = playerPrefs.GetString(PREVIOUS_SEARCHES_KEY, "");
        return string.IsNullOrEmpty(previousSearches) ? Array.Empty<string>() : previousSearches.Split('|');
    }
}
