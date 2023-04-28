using DCL.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Backpack
{
    public class BackpackFiltersController
    {
        public event Action<bool> OnOnlyCollectiblesChanged;
        public event Action<HashSet<string>> OnCollectionChanged;
        public event Action<string> OnSortByChanged;
        public event Action<string> OnSearchTextChanged;

        private readonly IBackpackFiltersComponentView view;
        private bool collectionsAlreadyLoaded;

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

        public void LoadCollections()
        {
            if (collectionsAlreadyLoaded)
                return;

            WearablesFetchingHelper.GetThirdPartyCollections()
                                   .Then(collections =>
                                    {
                                        WearableCollectionsAPIData.Collection defaultCollection = new () { urn = "decentraland", name = "Decentraland",};
                                        view.LoadCollectionDropdown(collections, defaultCollection);
                                        collectionsAlreadyLoaded = true;
                                    })
                                   .Catch((error) => Debug.LogError(error));
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
