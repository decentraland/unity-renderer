using DCLServices.WearablesCatalogService;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL.Backpack
{
    public class BackpackFiltersComponentView : MonoBehaviour, IBackpackFiltersComponentView
    {
        public event Action<bool> OnOnlyCollectiblesChanged;
        public event Action<HashSet<string>> OnCollectionChanged;
        public event Action<(NftOrderByOperation type, bool directionAscendent)> OnSortByChanged;
        public event Action<string> OnSearchTextChanged;

        public List<ToggleComponentModel> loadedFilters;

        [SerializeField] internal ToggleComponentView onlyCollectiblesToggle;
        [SerializeField] internal DropdownComponentView collectionDropdown;
        [SerializeField] internal DropdownComponentView sortByDropdown;
        [SerializeField] internal SearchBarComponentView searchBar;

        private readonly HashSet<string> selectedCollections = new ();

        private const string NEWEST_FILTER_ID = "newest";
        private const string OLDEST_FILTER_ID = "oldest";
        private const string RAREST_FILTER_ID = "rarest";
        private const string LESS_RARE_FILTER_ID = "less_rare";
        private const string NAME_AZ_FILTER_ID = "name_az";
        private const string NAME_ZA_FILTER_ID = "name_za";

        private void Awake()
        {
            LoadSortByDropdown();

            onlyCollectiblesToggle.OnSelectedChanged += OnOnlyCollectiblesToggleChanged;
            collectionDropdown.OnOptionSelectionChanged += OnCollectionDropdownChanged;
            sortByDropdown.OnOptionSelectionChanged += OnSortByDropdownChanged;
            searchBar.OnSearchText += OnSearchBarChanged;
        }

        public void Dispose()
        {
            onlyCollectiblesToggle.OnSelectedChanged -= OnOnlyCollectiblesToggleChanged;
            collectionDropdown.OnOptionSelectionChanged -= OnCollectionDropdownChanged;
            sortByDropdown.OnOptionSelectionChanged -= OnSortByDropdownChanged;
            searchBar.OnSearchText -= OnSearchBarChanged;
        }

        public void LoadCollectionDropdown(
            WearableCollectionsAPIData.Collection[] collections,
            WearableCollectionsAPIData.Collection defaultCollection = null)
        {
            List<ToggleComponentModel> collectionsToAdd = new ();

            if (defaultCollection != null)
            {
                ToggleComponentModel defaultToggle = new ()
                {
                    id = defaultCollection.urn,
                    text = defaultCollection.name,
                    isOn = true,
                    isTextActive = true,
                };

                collectionsToAdd.Add(defaultToggle);
                selectedCollections.Add(defaultToggle.id);
            }

            foreach (var collection in collections)
            {
                ToggleComponentModel newCollectionModel = new ToggleComponentModel
                {
                    id = collection.urn,
                    text = collection.name,
                    isOn = false,
                    isTextActive = true,
                };

                collectionsToAdd.Add(newCollectionModel);
            }

            collectionDropdown.SetOptions(collectionsToAdd);
            loadedFilters = collectionsToAdd;
        }

        private void LoadSortByDropdown()
        {
            List<ToggleComponentModel> sortingMethodsToAdd = new List<ToggleComponentModel>
            {
                new () { id = NEWEST_FILTER_ID, text = "Newest", isOn = false, isTextActive = true, changeTextColorOnSelect = true },
                new () { id = OLDEST_FILTER_ID, text = "Oldest", isOn = false, isTextActive = true, changeTextColorOnSelect = true },
                new () { id = RAREST_FILTER_ID, text = "Rarest", isOn = false, isTextActive = true, changeTextColorOnSelect = true },
                new () { id = LESS_RARE_FILTER_ID, text = "Less rare", isOn = false, isTextActive = true, changeTextColorOnSelect = true },
                new () { id = NAME_AZ_FILTER_ID, text = "Name A-Z", isOn = true, isTextActive = true, changeTextColorOnSelect = true },
                new () { id = NAME_ZA_FILTER_ID, text = "Name Z-A", isOn = false, isTextActive = true, changeTextColorOnSelect = true },
            };

            sortByDropdown.SetTitle(sortingMethodsToAdd[4].text);
            sortByDropdown.SetOptions(sortingMethodsToAdd);
        }

        private void OnOnlyCollectiblesToggleChanged(bool isOn, string optionId, string optionText) =>
            OnOnlyCollectiblesChanged?.Invoke(isOn);

        private void OnCollectionDropdownChanged(bool isOn, string optionId, string optionName)
        {
            if (isOn)
            {
                if (selectedCollections.Contains(optionId))
                    return;

                selectedCollections.Add(optionId);
            }
            else
                selectedCollections.Remove(optionId);

            // need to make a copy of the collection because it may be modified in the event subscription
            OnCollectionChanged?.Invoke(selectedCollections.ToHashSet());
        }

        private void OnSortByDropdownChanged(bool isOn, string optionId, string optionName)
        {
            if (!isOn)
                return;

            sortByDropdown.SetTitle(optionName);

            switch (optionId)
            {
                case OLDEST_FILTER_ID:
                    OnSortByChanged?.Invoke((NftOrderByOperation.Date, true));
                    break;
                case NEWEST_FILTER_ID:
                    OnSortByChanged?.Invoke((NftOrderByOperation.Date, false));
                    break;
                case LESS_RARE_FILTER_ID:
                    OnSortByChanged?.Invoke((NftOrderByOperation.Rarity, true));
                    break;
                case RAREST_FILTER_ID:
                    OnSortByChanged?.Invoke((NftOrderByOperation.Rarity, false));
                    break;
                case NAME_AZ_FILTER_ID:
                    OnSortByChanged?.Invoke((NftOrderByOperation.Name, true));
                    break;
                case NAME_ZA_FILTER_ID:
                    OnSortByChanged?.Invoke((NftOrderByOperation.Name, false));
                    break;
            }
        }

        private void OnSearchBarChanged(string newText) =>
            OnSearchTextChanged?.Invoke(newText);
    }
}
