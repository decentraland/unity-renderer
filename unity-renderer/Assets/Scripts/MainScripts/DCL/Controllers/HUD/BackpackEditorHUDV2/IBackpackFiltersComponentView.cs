using DCLServices.WearablesCatalogService;
using System;
using System.Collections.Generic;

namespace DCL.Backpack
{
    public interface IBackpackFiltersComponentView
    {
        event Action<bool> OnOnlyCollectiblesChanged;
        event Action<HashSet<string>> OnCollectionChanged;
        event Action<(NftOrderByOperation type, bool directionAscendent)> OnSortByChanged;
        event Action<string> OnSearchTextChanged;

        void Dispose();
        void LoadCollectionDropdown(WearableCollectionsAPIData.Collection[] collections, WearableCollectionsAPIData.Collection defaultCollection = null);
        void SetSearchText(string text, bool notify);
        void SetOnlyCollectiblesToggleIsOn(bool isOn, bool notify);
        void SetSorting(NftOrderByOperation type, bool directionAscending, bool notify);
        void SelectDropdownCollections(HashSet<string> collections, bool notify);
    }
}
