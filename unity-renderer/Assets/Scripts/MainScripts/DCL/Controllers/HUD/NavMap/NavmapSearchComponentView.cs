using MainScripts.DCL.Controllers.HotScenes;
using MainScripts.DCL.Helpers.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

public class NavmapSearchComponentView : BaseComponentView, INavmapSearchComponentView
{
    private const int MAX_RECORDS_COUNT = 5;

    [SerializeField] private SearchBarComponentView searchBar;
    [SerializeField] internal SearchRecordComponentView recordPrefab;
    [SerializeField] internal Transform recordsParent;

    public event Action<bool> OnSelectedSearchBar;
    public event Action<string> OnSearchedText;

    private UnityObjectPool<SearchRecordComponentView> recordsPool;
    private readonly List<SearchRecordComponentView> usedRecords = new ();

    public void Start()
    {
        searchBar.OnSelected += OnSearchBarSelected;
        searchBar.OnSearchText += OnSearchText;
        searchBar.OnSearchValueChanged += OnSearchValueChanged;

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

        for(int i = previousSearches.Length - 1; i >= 0; i--)
        {
            SearchRecordComponentView searchRecordComponentView = recordsPool.Get();
            searchRecordComponentView.OnSelectedHistoryRecord -= OnSelectedHistoryRecord;
            searchRecordComponentView.OnSelectedHistoryRecord += OnSelectedHistoryRecord;
            searchRecordComponentView.SetModel(new SearchRecordComponentModel()
            {
                recordText = previousSearches[i],
                isHistory = true
            });
            usedRecords.Add(searchRecordComponentView);
        }
    }

    public void SetSearchResultRecords(IReadOnlyList<IHotScenesController.PlaceInfo> places)
    {
        ClearResults();

        foreach (IHotScenesController.PlaceInfo placeInfo in places)
        {
            SearchRecordComponentView searchRecordComponentView = recordsPool.Get();
            searchRecordComponentView.SetModel(new SearchRecordComponentModel()
            {
                recordText = placeInfo.title,
                isHistory = false
            });
            usedRecords.Add(searchRecordComponentView);
        }
    }

    private void OnSelectedHistoryRecord(string searchText)
    {
        searchBar.SubmitSearch(searchText, true);
    }

    public void ClearResults()
    {
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
