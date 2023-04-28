using System;
using System.Collections.Generic;

namespace DCL.Backpack
{
    public class BackpackFiltersController
    {
        public event Action<bool> OnOnlyCollectiblesChanged;
        public event Action<HashSet<string>> OnCollectionChanged;
        public event Action<string> OnSortByChanged;
        public event Action<string> OnSearchTextChanged;

        private readonly IBackpackFiltersComponentView view;

        public BackpackFiltersController(IBackpackFiltersComponentView view)
        {
            this.view = view;

            view.OnOnlyCollectiblesChanged += ChangeOnlyCollectibles;
            view.OnCollectionChanged += ChangeOnCollection;
            view.OnSortByChanged += ChangeSortBy;
            view.OnSearchTextChanged += ChangeSearchText;
        }

        public void Dispose()
        {
            view.OnOnlyCollectiblesChanged -= ChangeOnlyCollectibles;
            view.OnCollectionChanged -= ChangeOnCollection;
            view.OnSortByChanged -= ChangeSortBy;
            view.OnSearchTextChanged -= ChangeSearchText;

            view.Dispose();
        }

        private void ChangeOnlyCollectibles(bool isOn) =>
            OnOnlyCollectiblesChanged?.Invoke(isOn);

        private void ChangeOnCollection(HashSet<string> selectedCollections) =>
            OnCollectionChanged?.Invoke(selectedCollections);

        private void ChangeSortBy(string newSorting) =>
            OnSortByChanged?.Invoke(newSorting);

        private void ChangeSearchText(string newText) =>
            OnSearchTextChanged?.Invoke(newText);
    }
}
