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

            OnSortByChanged?.Invoke(optionId);
        }

        private void OnSearchBarChanged(string newText) =>
            OnSearchTextChanged?.Invoke(newText);
    }
}
