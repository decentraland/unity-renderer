using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Backpack
{
    public class BackpackFiltersComponentView : MonoBehaviour, IBackpackFiltersComponentView
    {
        public event Action<bool> OnOnlyCollectiblesChanged;
        public event Action<HashSet<string>> OnCollectionChanged;
        public event Action<string> OnSortByChanged;
        public event Action<string> OnSearchTextChanged;

        [SerializeField] private ToggleComponentView onlyCollectiblesToggle;
        [SerializeField] private DropdownComponentView collectionDropdown;
        [SerializeField] private DropdownComponentView sortByDropdown;
        [SerializeField] private SearchBarComponentView searchBar;

        private readonly HashSet<string> selectedCollections = new ();

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
                    id = "decentraland",
                    text = "Decentraland",
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
        }

        private void LoadSortByDropdown()
        {
            List<ToggleComponentModel> sortingMethodsToAdd = new List<ToggleComponentModel>
            {
                new () { id = "newest", text = "Newest", isOn = false, isTextActive = true, changeTextColorOnSelect = true },
                new () { id = "oldest", text = "Oldest", isOn = false, isTextActive = true, changeTextColorOnSelect = true },
                new () { id = "rarest", text = "Rarest", isOn = false, isTextActive = true, changeTextColorOnSelect = true },
                new () { id = "less_rare", text = "Less rare", isOn = false, isTextActive = true, changeTextColorOnSelect = true },
                new () { id = "name_az", text = "Name A-Z", isOn = true, isTextActive = true, changeTextColorOnSelect = true },
                new () { id = "name_za", text = "Name Z-A", isOn = false, isTextActive = true, changeTextColorOnSelect = true },
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

            OnCollectionChanged?.Invoke(selectedCollections);
        }

        private void OnSortByDropdownChanged(bool isOn, string optionId, string optionName)
        {
            if (!isOn)
                return;

            sortByDropdown.SetTitle(optionName);
            OnSortByChanged?.Invoke(optionId);
        }

        private void OnSearchBarChanged(string newText) =>
            OnSearchTextChanged?.Invoke(newText);
    }
}
