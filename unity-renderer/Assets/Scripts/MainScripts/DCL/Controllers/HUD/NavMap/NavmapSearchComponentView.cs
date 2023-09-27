using MainScripts.DCL.Controllers.HotScenes;
using MainScripts.DCL.Helpers.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.UI;

public class NavmapSearchComponentView : BaseComponentView, INavmapSearchComponentView
{
    private const int MAX_RECORDS_COUNT = 5;

    [SerializeField] private SearchBarComponentView searchBar;
    [SerializeField] internal SearchRecordComponentView recordPrefab;
    [SerializeField] internal GameObject noRecordsFound;
    [SerializeField] internal Transform recordsParent;
    [SerializeField] internal GameObject searchResultsContainer;
    [SerializeField] internal Button closeButtonArea;

    public event Action<bool> OnSelectedSearchBar;
    public event Action<string> OnSearchedText;
    public event Action<Vector2Int> OnSelectedSearchRecord;

    private UnityObjectPool<SearchRecordComponentView> recordsPool;
    internal readonly List<SearchRecordComponentView> usedRecords = new ();

    public override void Awake()
    {
        base.Awake();

        searchBar.OnSelected += OnSearchBarSelected;
        searchBar.OnSearchText += OnSearchText;
        searchBar.OnSearchValueChanged += OnSearchValueChanged;
        closeButtonArea.onClick.RemoveAllListeners();
        closeButtonArea.onClick.AddListener(()=> OnSelectedSearchBar?.Invoke(false));
        recordsPool = new UnityObjectPool<SearchRecordComponentView>(recordPrefab, recordsParent);
        recordsPool.Prewarm(MAX_RECORDS_COUNT);
    }

    private void OnSearchValueChanged(string obj)
    {
        ClearResults();
    }

    private void OnSearchText(string searchText)
    {
        OnSearchedText?.Invoke(searchText);
    }

    private void OnSearchBarSelected(bool isOnFocus)
    {
        OnSelectedSearchBar?.Invoke(isOnFocus);
    }

    public void SetHistoryRecords(string[] previousSearches)
    {
        ClearResults();
        noRecordsFound.SetActive(false);
        searchResultsContainer.SetActive(true);

        for(int i = 0; i <= previousSearches.Length - 1; i++)
        {
            SearchRecordComponentView searchRecordComponentView = recordsPool.Get();
            searchRecordComponentView.OnSelectedHistoryRecord -= OnSelectedHistoryRecord;
            searchRecordComponentView.OnSelectedHistoryRecord += OnSelectedHistoryRecord;
            searchRecordComponentView.SetModel(new SearchRecordComponentModel()
            {
                recordText = previousSearches[i],
                isHistory = true,
                playerCount = 0
            });
            usedRecords.Add(searchRecordComponentView);
            searchRecordComponentView.transform.SetAsLastSibling();
        }
    }

    public void SetSearchResultRecords(IReadOnlyList<IHotScenesController.PlaceInfo> places)
    {
        ClearResults();
        searchResultsContainer.SetActive(true);
        noRecordsFound.SetActive(places.Count == 0);

        foreach (IHotScenesController.PlaceInfo placeInfo in places)
        {
            SearchRecordComponentView searchRecordComponentView = recordsPool.Get();
            searchRecordComponentView.OnSelectedRegularRecord -= OnSelectedRegularRecord;
            searchRecordComponentView.OnSelectedRegularRecord += OnSelectedRegularRecord;
            searchRecordComponentView.SetModel(new SearchRecordComponentModel()
            {
                recordText = placeInfo.title,
                isHistory = false,
                playerCount = placeInfo.user_count,
                placeCoordinates = DCL.Helpers.Utils.ConvertStringToVector(placeInfo.base_position)
            });
            usedRecords.Add(searchRecordComponentView);
        }
    }

    private void OnSelectedRegularRecord(Vector2Int coordinates)
    {
        OnSelectedSearchRecord?.Invoke(coordinates);
        searchBar.ClearSearch(false);
    }

    private void OnSelectedHistoryRecord(string searchText) =>
        searchBar.SubmitSearch(searchText, true, false);

    public void ClearResults()
    {
        searchResultsContainer.SetActive(false);
        foreach (var pooledRecord in usedRecords)
            recordsPool.Release(pooledRecord);

        usedRecords.Clear();
    }

    public override void RefreshControl()
    {
    }

    public override void Dispose()
    {
        base.Dispose();

        searchBar.onFocused -= OnSearchBarSelected;
        searchBar.OnSearchText -= OnSearchText;
        searchBar.OnSearchValueChanged -= OnSearchValueChanged;

        foreach (var pooledRecord in usedRecords)
            recordsPool.Release(pooledRecord);
        recordsPool.Clear();
    }
}
