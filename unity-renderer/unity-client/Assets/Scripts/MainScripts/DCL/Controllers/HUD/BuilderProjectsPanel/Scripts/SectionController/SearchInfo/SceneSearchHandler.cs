    using System;
    using System.Collections.Generic;

    internal class SceneSearchHandler : ISectionSearchHandler
    {
        public const string NAME_SORT_TYPE = "NAME";
        public const string SIZE_SORT_TYPE = "SIZE";

        private readonly string[] scenesSortTypes = { NAME_SORT_TYPE, SIZE_SORT_TYPE };

        public event Action OnUpdated;
        public event Action<List<SceneSearchInfo>> OnResult;

        private SearchHandler<SceneSearchInfo> scenesSearchHandler;

        private bool filterOwner = false;
        private bool filterOperator = false;
        private bool filterContributor = false;

        public SceneSearchHandler()
        {
            scenesSearchHandler = new SearchHandler<SceneSearchInfo>(scenesSortTypes, (item) =>
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

            scenesSearchHandler.OnSearchChanged += list =>
            {
                OnUpdated?.Invoke();
                OnResult?.Invoke(list);
            };
        }

        public void SetSearchableList(List<SceneSearchInfo> list)
        {
            scenesSearchHandler.SetSearchableList(list);
        }

        public void AddItem(SceneSearchInfo item)
        {
            scenesSearchHandler.AddItem(item);
        }

        public void RemoveItem(SceneSearchInfo item)
        {
            scenesSearchHandler.RemoveItem(item);
        }

        string[] ISectionSearchHandler.sortTypes => scenesSortTypes;
        string ISectionSearchHandler.searchString => scenesSearchHandler.currentSearchString;
        bool ISectionSearchHandler.filterOwner => filterOwner;
        bool ISectionSearchHandler.filterOperator => filterOperator;
        bool ISectionSearchHandler.filterContributor => filterContributor;
        bool ISectionSearchHandler.descendingSortOrder => scenesSearchHandler.isDescendingSortOrder;
        string ISectionSearchHandler.sortType => scenesSearchHandler.currentSortingType;
        int ISectionSearchHandler.resultCount => scenesSearchHandler.resultCount;

        void ISectionSearchHandler.SetFilter(bool isOwner, bool isOperator, bool isContributor)
        {
            filterOwner = isOwner;
            filterOperator = isOperator;
            filterContributor = isContributor;
            scenesSearchHandler.NotifyFilterChanged();
        }

        void ISectionSearchHandler.SetSortType(string sortType)
        {
            scenesSearchHandler.NotifySortTypeChanged(sortType);
        }

        void ISectionSearchHandler.SetSortOrder(bool isDescending)
        {
            scenesSearchHandler.NotifySortOrderChanged(isDescending);
        }

        void ISectionSearchHandler.SetSearchString(string searchText)
        {
            scenesSearchHandler.NotifySearchChanged(searchText);
        }
    }
