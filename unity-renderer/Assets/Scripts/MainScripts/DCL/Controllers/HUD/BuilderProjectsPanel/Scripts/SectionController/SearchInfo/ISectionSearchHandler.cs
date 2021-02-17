using System;

internal interface ISectionSearchHandler
{
    event Action OnUpdated;
    string[] sortTypes { get; }
    string searchString { get; }
    bool filterOwner { get; }
    bool filterOperator { get; }
    bool filterContributor { get; }
    bool descendingSortOrder { get; }
    string sortType { get; }
    int resultCount { get; }
    void SetFilter(bool isOwner, bool isOperator, bool isContributor);
    void SetSortType(string sortType);
    void SetSortOrder(bool isDescending);
    void SetSearchString(string searchText);
}
