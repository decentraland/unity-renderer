using System;
using System.Collections.Generic;

public class SearchHandler<T> where T: ISearchable, ISortable<T>
{
    public event Action<List<T>> OnSearchChanged;

    public string[] sortingTypes { get; }
    public bool isDescendingSortOrder { get; private set; } = true;
    public string currentSearchString { get; private set; }
    public string currentSortingType { get; private set; }
    public int resultCount { get { return currentResult?.Count ?? 0; } }

    private List<T> originalList;
    private List<T> searchResult;
    private List<T> currentResult;

    private readonly Predicate<T> currentFilterPredicate;

    private bool searchStringChanged = false;
    private bool searchSortChanged = false;
    private bool searchFilterChanged = false;

    public SearchHandler(string[] sortingTypes, Predicate<T> filterPredicate)
    {
        this.sortingTypes = sortingTypes;
        this.currentFilterPredicate = filterPredicate;
        this.currentSortingType = sortingTypes[0];
    }

    public void SetSearchableList(List<T> list)
    {
        originalList = list;
        searchResult = list;
        currentResult = list;

        searchStringChanged = true;
        searchFilterChanged = true;
        searchSortChanged = true;

        GetResult(OnSearchChanged);
    }

    public void AddItem(T item)
    {
        originalList.Add(item);
        bool matchSearch = MatchSearch(item);
        bool matchResult = matchSearch && MatchFilter(item);

        if (matchSearch)
        {
            searchResult.Add(item);
        }

        if (matchResult)
        {
            currentResult.Add(item);
            searchSortChanged = true;
            GetResult(OnSearchChanged);
        }
    }

    public void RemoveItem(T item)
    {
        originalList.Remove(item);
        searchResult.Remove(item);
        bool inResult = currentResult.Remove(item);
        if (inResult)
        {
            OnSearchChanged?.Invoke(currentResult);
        }
    }

    public void GetResult(Action<List<T>> onResult)
    {
        if (searchStringChanged)
        {
            searchResult = string.IsNullOrEmpty(currentSearchString) ? originalList : SearchHelper.Search(currentSearchString, originalList);
            currentResult = searchResult;
        }

        if (searchFilterChanged || searchStringChanged)
        {
            currentResult = currentFilterPredicate != null
                ? SearchHelper.Filter(searchResult, currentFilterPredicate)
                : searchResult;
        }

        if (searchSortChanged || searchStringChanged)
        {
            SearchHelper.Sort(currentSortingType, currentResult, isDescendingSortOrder);
        }

        searchStringChanged = false;
        searchFilterChanged = false;
        searchSortChanged = false;

        onResult?.Invoke(currentResult);
    }

    public void NotifySearchChanged(string searchText)
    {
        searchStringChanged = searchStringChanged || searchText != currentSearchString;
        currentSearchString = searchText;
        GetResult(OnSearchChanged);
    }

    public void NotifySortTypeChanged(string sortingType)
    {
        searchSortChanged = searchSortChanged || currentSortingType != sortingType;
        currentSortingType = sortingType;
        GetResult(OnSearchChanged);
    }

    public void NotifySortOrderChanged(bool isDescending)
    {
        searchSortChanged = searchSortChanged || isDescendingSortOrder != isDescending;
        isDescendingSortOrder = isDescending;
        GetResult(OnSearchChanged);
    }

    public void NotifyFilterChanged()
    {
        searchFilterChanged = true;
        GetResult(OnSearchChanged);
    }

    public bool Match(T item)
    {
        return MatchSearch(item) && MatchFilter(item);
    }

    private bool MatchSearch(T item)
    {
        return SearchHelper.SearchMatchItem(currentSearchString, item);
    }

    private bool MatchFilter(T item)
    {
        if (currentFilterPredicate == null)
            return true;

        return currentFilterPredicate(item);
    }
}
