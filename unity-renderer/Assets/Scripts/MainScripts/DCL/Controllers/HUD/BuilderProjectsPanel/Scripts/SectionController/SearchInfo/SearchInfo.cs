using System;
using UnityEngine;

internal interface ISearchInfo : ISearchable, ISortable<ISearchInfo>
{
    string id { get; }
    string name { get; }
    int size { get; }
    bool isOwner { get; }
    bool isOperator { get; }
    bool isContributor { get; }
    void SetId(string id);
    void SetName(string name);
    void SetSize(int size);
    void SetCoords(string coords);
    void SetRole(bool isOwner);
}

internal class SearchInfo : ISearchInfo
{
    string ISearchInfo.id => id;
    string ISearchInfo.name => name;
    int ISearchInfo.size => size;
    bool ISearchInfo.isOwner => isOwner;
    bool ISearchInfo.isOperator => isOperator;
    bool ISearchInfo.isContributor => isContributor;
    string[] ISearchable.keywords => keywords;

    private bool isOwner;
    private bool isOperator;
    private bool isContributor;
    private string id;
    private string name;
    private int size;
    private string[] keywords;

    public SearchInfo()
    {
        keywords = new string[2];
    }

    void ISearchInfo.SetId(string id)
    {
        this.id = id;
    }
    
    void ISearchInfo.SetName(string name)
    {
        this.name = name;
        keywords[0] = name;
    }

    void ISearchInfo.SetSize(int size)
    {
        this.size = size;
    }

    void ISearchInfo.SetCoords(string coords)
    {
        keywords[1] = coords;
    }

    void ISearchInfo.SetRole(bool isOwner)
    {
        this.isOwner = isOwner;
        this.isOperator = !isOwner;
        this.isContributor = false;
    }

    int ISortable<ISearchInfo>.Compare(string sortType, bool isDescendingOrder, ISearchInfo other)
    {
        switch (sortType)
        {
            case SectionSearchHandler.NAME_SORT_TYPE_ASC:
                return String.CompareOrdinal(name, other.name);
            case SectionSearchHandler.NAME_SORT_TYPE_DESC:
                return String.CompareOrdinal(other.name, name);
            case SectionSearchHandler.SIZE_SORT_TYPE_DESC:
                return other.size - size;
            case SectionSearchHandler.SIZE_SORT_TYPE_ASC:
                return size - other.size;
            default:
                return 0;
        }
    }
}