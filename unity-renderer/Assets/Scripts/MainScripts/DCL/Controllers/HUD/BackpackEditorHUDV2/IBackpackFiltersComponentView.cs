using System;
using System.Collections.Generic;

namespace DCL.Backpack
{
    public interface IBackpackFiltersComponentView
    {
        event Action<bool> OnOnlyCollectiblesChanged;
        event Action<HashSet<string>> OnCollectionChanged;
        event Action<string> OnSortByChanged;
        event Action<string> OnSearchTextChanged;

        void Dispose();
        void LoadCollectionDropdown(WearableCollectionsAPIData.Collection[] collections, WearableCollectionsAPIData.Collection defaultCollection = null);
    }
}
