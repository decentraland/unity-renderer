using System;
using System.Collections.Generic;

internal class LandSearchHandler : ISectionSearchHandler
{
    public const string NAME_SORT_TYPE = "NAME";
    public const string SIZE_SORT_TYPE = "SIZE";

    private readonly string[] landSortTypes = { NAME_SORT_TYPE, SIZE_SORT_TYPE };

    public event Action OnUpdated;
    public event Action<List<LandSearchInfo>> OnResult;
    
    string[] ISectionSearchHandler.sortTypes => landSortTypes;
    string ISectionSearchHandler.searchString => landSearchHandler.currentSearchString;
    bool ISectionSearchHandler.filterOwner => filterOwner;
    bool ISectionSearchHandler.filterOperator => filterOperator;
    bool ISectionSearchHandler.filterContributor => filterContributor;
    bool ISectionSearchHandler.descendingSortOrder => landSearchHandler.isDescendingSortOrder;
    string ISectionSearchHandler.sortType => landSearchHandler.currentSortingType;
    int ISectionSearchHandler.resultCount => landSearchHandler.resultCount;

    private SearchHandler<LandSearchInfo> landSearchHandler;

    private bool filterOwner = false;
    private bool filterOperator = false;
    private bool filterContributor = false;

    public LandSearchHandler()
    {
        landSearchHandler = new SearchHandler<LandSearchInfo>(landSortTypes, (item) =>
        {
            bool result = true;
            if (filterContributor)
                result = item.isContributor;
            if (filterOperator && result)
                result = item.isOperator;
            if (filterOwner && result)
                result = item.isOwner;
            return result;
        });

        landSearchHandler.OnSearchChanged += list =>
        {
            OnUpdated?.Invoke();
            OnResult?.Invoke(list);
        };
    }

    public void SetSearchableList(List<LandSearchInfo> list) { landSearchHandler.SetSearchableList(list); }

    public void AddItem(LandSearchInfo item) { landSearchHandler.AddItem(item); }

    public void RemoveItem(LandSearchInfo item) { landSearchHandler.RemoveItem(item); }

    void ISectionSearchHandler.SetFilter(bool isOwner, bool isOperator, bool isContributor)
    {
        filterOwner = isOwner;
        filterOperator = isOperator;
        filterContributor = isContributor;
        landSearchHandler.NotifyFilterChanged();
    }

    void ISectionSearchHandler.SetSortType(string sortType) { landSearchHandler.NotifySortTypeChanged(sortType); }

    void ISectionSearchHandler.SetSortOrder(bool isDescending) { landSearchHandler.NotifySortOrderChanged(isDescending); }

    void ISectionSearchHandler.SetSearchString(string searchText) { landSearchHandler.NotifySearchChanged(searchText); }
}