    using System;
    using UnityEngine;

    internal class SceneSearchInfo : ISearchable, ISortable<SceneSearchInfo>
    {
        public string id;
        public string name { get; private set; }
        public int size { get; private set; }
        public bool isOwner;
        public bool isOperator;
        public bool isContributor;

        public string[] keywords { get; }

        public SceneSearchInfo()
        {
            keywords = new string[2];
        }

        public void SetName(string name)
        {
            this.name = name;
            keywords[0] = name;
        }

        public void SetSize(Vector2Int size)
        {
            this.size = size.x * size.y;
            keywords[1] = this.size.ToString();
        }

        public void SetRole(bool isOwner, bool isOperator, bool isContributor)
        {
            this.isOwner = isOwner;
            this.isOperator = isOperator;
            this.isContributor = isContributor;
        }

        public int Compare(string sortType, bool isDescendingOrder, SceneSearchInfo other)
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
