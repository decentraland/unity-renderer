using System;
using UnityEngine;

internal class LandSearchInfo : ISearchable, ISortable<LandSearchInfo>
{
    public string id;
    public string name { get; private set; }
    public int size { get; private set; }
    public bool isOwner;
    public bool isOperator;
    public bool isContributor;

    public string[] keywords { get; }

    public LandSearchInfo()
    {
        keywords = new string[2];
    }

    public void SetName(string name)
    {
        this.name = name;
        keywords[0] = name;
    }

    public void SetSize(int size)
    {
        this.size = size;
        keywords[1] = this.size.ToString();
    }

    public void SetRole(bool isOwner)
    {
        this.isOwner = isOwner;
        this.isOperator = !isOwner;
        this.isContributor = false;
    }

    public int Compare(string sortType, bool isDescendingOrder, LandSearchInfo other)
    {
        switch (sortType)
        {
            case SceneSearchHandler.NAME_SORT_TYPE when isDescendingOrder:
                return String.CompareOrdinal(name, other.name);
            case SceneSearchHandler.NAME_SORT_TYPE:
                return String.CompareOrdinal(other.name, name);
            case SceneSearchHandler.SIZE_SORT_TYPE when isDescendingOrder:
                return other.size - size;
            case SceneSearchHandler.SIZE_SORT_TYPE:
                return size - other.size;
            default:
                return 0;
        }
    }
}