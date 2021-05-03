using System;
using System.Collections.Generic;

internal interface ISectionSearchHandler
{
    event Action OnUpdated;
    event Action<List<ISearchInfo>> OnResult;
    string[] sortTypes { get; }
    string searchString { get; }
    bool filterOwner { get; }
    bool filterOperator { get; }
    bool filterContributor { get; }
    string sortType { get; }
    int resultCount { get; }
    void SetFilter(bool isOwner, bool isOperator, bool isContributor);
    void SetSortType(string sortType);
    void SetSearchString(string searchText);
    void SetSearchableList(List<ISearchInfo> list);
    void AddItem(ISearchInfo item);
    void RemoveItem(ISearchInfo item);
}
