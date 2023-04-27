using System;
using System.Collections.Generic;

namespace DCL.Backpack
{
    public class BackpackFiltersController
    {
        public event Action<bool> OnOnlyCollectibleChanged;
        public event Action<HashSet<string>> CollectionChanged;
        public event Action<string> SortByChanged;
        public event Action<string> SearchTextChanged;

        private readonly IBackpackFiltersComponentView view;

        public BackpackFiltersController(IBackpackFiltersComponentView view)
        {
            this.view = view;

            view.OnOnlyCollectiblesChanged += OnlyCollectiblesChanged;
            view.OnCollectionChanged += OnCollectionChanged;
            view.OnSortByChanged += OnSortByChanged;
            view.OnSearchTextChanged += OnSearchTextChanged;
        }

        public void Dispose()
        {
            view.OnOnlyCollectiblesChanged -= OnlyCollectiblesChanged;
            view.OnCollectionChanged -= OnCollectionChanged;
            view.OnSortByChanged -= OnSortByChanged;
            view.OnSearchTextChanged -= OnSearchTextChanged;

            view.Dispose();
        }

        private void OnlyCollectiblesChanged(bool isOn) =>
            OnOnlyCollectibleChanged?.Invoke(isOn);

        private void OnCollectionChanged(HashSet<string> selectedCollections) =>
            CollectionChanged?.Invoke(selectedCollections);

        private void OnSortByChanged(string newSorting) =>
            SortByChanged?.Invoke(newSorting);

        private void OnSearchTextChanged(string newText) =>
            SearchTextChanged?.Invoke(newText);
    }
}
